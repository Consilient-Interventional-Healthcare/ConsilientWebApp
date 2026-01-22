import alasql from 'alasql';
import db from './db.json';
import type { DbSchema } from '../types/db.types';
import type { Visits } from '../types/api.generated';

const typedDb = db as unknown as DbSchema;
type TableName = keyof DbSchema;

function getTodayDateString(): string {
  return new Date().toISOString().split('T')[0] ?? '';
}

export class DataProvider {
  constructor() {
    this.initTables();
  }

  private initTables() {
    Object.keys(typedDb).forEach((key) => {
      const table = key as TableName;
      alasql(`CREATE TABLE IF NOT EXISTS ${table}`);
      if (alasql.tables[table]) {
        // If loading visits, replace their date with today
        if (table === "visits" && Array.isArray(typedDb[table])) {
          alasql.tables[table].data = ((typedDb[table] as unknown) as Visits.VisitDto[]).map(v => ({
            ...v,
            date: getTodayDateString()
          }));
        } else {
          alasql.tables[table].data = typedDb[table];
        }
      }
    });
  }

  query<T>(sql: string, params?: unknown[]): T[] {
    console.log('Executing SQL:', sql, 'with params:', params);
    return alasql(sql, params);
  }

  getTable<T>(table: TableName): T[] {
    return (alasql.tables[table]?.data as T[]) ?? [];
  }

  insert<T>(table: TableName, record: T): void {
    alasql(`INSERT INTO ${table} VALUES (?)`, [record]);
  }
}

export const dataProvider = new DataProvider();