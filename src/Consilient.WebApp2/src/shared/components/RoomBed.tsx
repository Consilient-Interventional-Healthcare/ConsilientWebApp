interface RoomBedProps {
  room: string | null | undefined;
  bed: string | null | undefined;
  className?: string;
}

/**
 * Displays room and bed values in a consistent space-separated format.
 * Renders nothing if both values are empty.
 * Styling inherits from parent - use className for custom styles.
 *
 * @example
 * ```tsx
 * <RoomBed room="101" bed="A" />                    // Renders: "101 A"
 * <RoomBed room="101" bed={null} />                 // Renders: "101"
 * <RoomBed room={null} bed={null} />                // Renders: nothing
 * <RoomBed room="101" bed="A" className="text-sm" /> // Renders: "101 A" with custom class
 * ```
 */
export function RoomBed({ room, bed, className }: RoomBedProps) {
  const formatted = [room, bed].filter(Boolean).join(' ');
  if (!formatted) return null;
  return <span className={className}>{formatted}</span>;
}
