import React from "react";
import {
  ComposedChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  ResponsiveContainer,
  ReferenceLine,
  Scatter,
} from "recharts";
import { Clock } from "lucide-react";

export default function PatientProgressGraph(): React.ReactElement {
  const MILLISECONDS_PER_HOUR = 3600000;
  const MILLISECONDS_PER_DAY = 24 * MILLISECONDS_PER_HOUR;
  const baseTime: number = new Date("2024-11-20T08:00:00").getTime();

  interface PatientDataPoint {
    time: number;
    status: string;
    statusLevel: number;
    notes: string;
    isEvent: boolean;
    isPsychEval?: boolean;
    isDischarge?: boolean;
  }

  interface CustomShapeProps {
    cx?: number;
    cy?: number;
    payload?: PatientDataPoint;
    [key: string]: unknown;
  }

  interface StatusConfig {
    label: string;
    color: string;
  }

  interface StatusConfigs {
    [key: number]: StatusConfig;
    psych: StatusConfig;
    discharge: StatusConfig;
  }

  interface Segment {
    data: PatientDataPoint[];
    color: string;
    key: string;
    startTime: number;
    endTime: number;
    durationDays: number;
    statusLevel: number;
  }

  const data: PatientDataPoint[] = [
    {
      time: baseTime,
      status: "Psychiatric Evaluation",
      statusLevel: 3,
      notes: "Initial mental health assessment",
      isEvent: true,
      isPsychEval: true,
    },
    {
      time: baseTime,
      status: "Acute",
      statusLevel: 3,
      notes: "Admitted for acute care",
      isEvent: false,
    },
    {
      time: baseTime + 120 * MILLISECONDS_PER_HOUR,
      status: "Acute",
      statusLevel: 3,
      notes: "Acute care complete",
      isEvent: false,
    },
    {
      time: baseTime + 120 * MILLISECONDS_PER_HOUR,
      status: "Status Next Day",
      statusLevel: 2,
      notes: "Stable, continuing observation",
      isEvent: false,
    },
    {
      time: baseTime + 144 * MILLISECONDS_PER_HOUR,
      status: "Status Next Day",
      statusLevel: 2,
      notes: "Observation complete",
      isEvent: false,
    },
    {
      time: baseTime + 144 * MILLISECONDS_PER_HOUR,
      status: "Pending Placement",
      statusLevel: 1,
      notes: "Awaiting bed assignment",
      isEvent: false,
    },
    {
      time: baseTime + 168 * MILLISECONDS_PER_HOUR,
      status: "Pending Placement",
      statusLevel: 1,
      notes: "Placement confirmed",
      isEvent: false,
    },
    {
      time: baseTime + 168 * MILLISECONDS_PER_HOUR,
      status: "Discharge Summary",
      statusLevel: 1,
      notes: "Patient discharged",
      isEvent: true,
      isDischarge: true,
    },
  ];

  const statusConfig: StatusConfigs = {
    3: { label: "Acute", color: "#dc2626" },
    2: { label: "Status Next Day", color: "#3b82f6" },
    1: { label: "Pending Placement", color: "#fbbf24" },
    psych: { label: "Psychiatric Evaluation", color: "#8b5cf6" },
    discharge: { label: "Discharge Summary", color: "#10b981" },
  };

  const segments: Segment[] = [];
  for (let i = 0; i < data.length - 1; i++) {
    const currentPoint = data[i];
    const nextPoint = data[i + 1];

    if (!currentPoint || !nextPoint) {
      continue;
    }

    if (!currentPoint.isEvent && !nextPoint.isEvent) {
      const startTime = currentPoint.time;
      const endTime = nextPoint.time;
      const durationDays = Math.round(
        (endTime - startTime) / MILLISECONDS_PER_DAY
      );
      segments.push({
        data: [currentPoint, nextPoint],
        color: statusConfig[currentPoint.statusLevel]?.color ?? "#000",
        key: `segment-${i}`,
        startTime,
        endTime,
        durationDays,
        statusLevel: currentPoint.statusLevel,
      });
    }
  }

  const statusChangeTimes: number[] = [
    baseTime,
    baseTime + 120 * MILLISECONDS_PER_HOUR,
    baseTime + 144 * MILLISECONDS_PER_HOUR,
    baseTime + 168 * MILLISECONDS_PER_HOUR,
  ];

  return (
    <div className="w-full bg-white rounded-lg border border-gray-200 shadow-sm p-4">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <Clock size={16} className="text-gray-600" />
          <span className="text-sm font-semibold text-gray-700">
            Patient Status Timeline
          </span>
        </div>
      </div>

      <div className="bg-gray-50 rounded-lg p-3">
        <ResponsiveContainer width="100%" height={350}>
          <ComposedChart
            data={data}
            margin={{ top: 10, right: 20, left: 10, bottom: 10 }}
          >
            <ReferenceLine
              y={3}
              stroke="#dc2626"
              strokeDasharray="3 3"
              strokeOpacity={0.3}
            />
            <ReferenceLine
              y={2}
              stroke="#3b82f6"
              strokeDasharray="3 3"
              strokeOpacity={0.3}
            />
            <ReferenceLine
              y={1}
              stroke="#fbbf24"
              strokeDasharray="3 3"
              strokeOpacity={0.3}
            />

            <CartesianGrid
              strokeDasharray="3 3"
              stroke="#e5e7eb"
              vertical={false}
            />

            <XAxis
              dataKey="time"
              type="number"
              domain={[
                baseTime - MILLISECONDS_PER_DAY,
                baseTime + 8 * MILLISECONDS_PER_DAY,
              ]}
              ticks={statusChangeTimes}
              tickFormatter={(ts: number) =>
                new Date(ts).toLocaleDateString("en-US", {
                  month: "short",
                  day: "numeric",
                })
              }
              stroke="#6b7280"
              style={{ fontSize: "12px" }}
            />

            <YAxis
              domain={[-0.5, 4.2]}
              ticks={[1, 2, 3]}
              tickFormatter={(v: number) =>
                statusConfig[v as keyof typeof statusConfig]?.label ?? ""
              }
              stroke="#6b7280"
              style={{ fontSize: "11px" }}
              width={140}
            />

            {/* Duration line at top */}
            <Line
              data={[
                { time: baseTime, duration: 3.8 },
                { time: baseTime + 7 * MILLISECONDS_PER_DAY, duration: 3.8 },
              ]}
              type="monotone"
              dataKey="duration"
              stroke="#6b7280"
              strokeWidth={2}
              dot={false}
            />

            {/* Small markers on duration line */}
            {statusChangeTimes.map((t: number, i: number) => (
              <Scatter
                key={`marker-${i}`}
                data={[{ time: t, duration: 3.8 }]}
                dataKey="duration"
                fill="#6b7280"
                shape={(props: unknown) => {
                  const { cx, cy } = props as CustomShapeProps;
                  if (cx !== undefined && cy !== undefined) {
                    return <circle cx={cx} cy={cy} r={3} fill="#6b7280" />;
                  }
                  // Always return a valid React element (empty group if no coordinates)
                  return <g />;
                }}
              />
            ))}

            {/* Duration labels on top line */}
            {segments.map((seg: Segment) => {
              const mid = (seg.startTime + seg.endTime) / 2;
              if (seg.durationDays === 0) return null;
              return (
                <ReferenceLine
                  key={`duration-label-${seg.key}`}
                  x={mid}
                  y={4.0}
                  stroke="transparent"
                  label={{
                    value: `${seg.durationDays}d`,
                    fill: seg.color,
                    fontSize: 11,
                    fontWeight: 700,
                  }}
                />
              );
            })}

            {/* Colored status lines */}
            {segments.map((seg: Segment) => (
              <Line
                key={seg.key}
                data={seg.data}
                type="stepAfter"
                dataKey="statusLevel"
                stroke={seg.color}
                strokeWidth={3}
                dot={false}
                isAnimationActive={false}
              />
            ))}

            {/* Psychiatric Evaluation event */}
            <Scatter
              data={data.filter((d: PatientDataPoint) => d.isPsychEval)}
              dataKey="statusLevel"
              fill={statusConfig.psych.color}
              shape={(props: unknown) => {
                const shapeProps = props as CustomShapeProps;
                return shapeProps.cx !== undefined &&
                  shapeProps.cy !== undefined ? (
                  <g>
                    <circle
                      cx={shapeProps.cx}
                      cy={shapeProps.cy}
                      r={6}
                      fill={statusConfig.psych.color}
                      stroke="white"
                      strokeWidth={3}
                    />
                    <circle
                      cx={shapeProps.cx}
                      cy={shapeProps.cy}
                      r={10}
                      fill="none"
                      stroke={statusConfig.psych.color}
                      strokeWidth={1.5}
                      opacity={0.5}
                    />
                    <text
                      x={shapeProps.cx - 15}
                      y={shapeProps.cy}
                      textAnchor="end"
                      dominantBaseline="middle"
                      fill={statusConfig.psych.color}
                      fontSize="11"
                      fontWeight="600"
                    >
                      Psych Eval
                    </text>
                  </g>
                ) : <g />;
              }}
            />

            {/* Discharge event */}
            <Scatter
              data={data.filter((d: PatientDataPoint) => d.isDischarge)}
              dataKey="statusLevel"
              fill={statusConfig.discharge.color}
              shape={(props: unknown) => {
                const shapeProps = props as CustomShapeProps;
                return shapeProps.cx !== undefined &&
                  shapeProps.cy !== undefined ? (
                  <g>
                    <circle
                      cx={shapeProps.cx}
                      cy={shapeProps.cy}
                      r={6}
                      fill={statusConfig.discharge.color}
                      stroke="white"
                      strokeWidth={3}
                    />
                    <circle
                      cx={shapeProps.cx}
                      cy={shapeProps.cy}
                      r={10}
                      fill="none"
                      stroke={statusConfig.discharge.color}
                      strokeWidth={1.5}
                      opacity={0.5}
                    />
                    <text
                      x={shapeProps.cx + 15}
                      y={shapeProps.cy}
                      textAnchor="start"
                      dominantBaseline="middle"
                      fill={statusConfig.discharge.color}
                      fontSize="11"
                      fontWeight="600"
                    >
                      Discharge
                    </text>
                  </g>
                ) : <g />;
              }}
            />

            {/* Regular dots on status lines */}
            <Line
              type="stepAfter"
              dataKey="statusLevel"
              stroke="transparent"
              strokeWidth={0}
              dot={(props: CustomShapeProps) => {
                const { cx, cy, payload } = props;
                if (
                  !payload ||
                  payload.isPsychEval ||
                  payload.isDischarge ||
                  cx === undefined ||
                  cy === undefined
                )
                  return null;
                const config =
                  statusConfig[payload.statusLevel as keyof StatusConfigs];
                if (!config) return null;
                return (
                  <circle
                    cx={cx}
                    cy={cy}
                    r={4}
                    fill={config.color}
                    stroke="white"
                    strokeWidth={2}
                  />
                );
              }}
            />
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
