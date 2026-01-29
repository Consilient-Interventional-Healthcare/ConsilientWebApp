import { useCallback } from 'react';

const VISIT_STORAGE_KEY = 'dailylog-visit-memory';
const PROVIDER_STORAGE_KEY = 'dailylog-provider-memory';

interface MemoryMap {
  [key: string]: number;
}

function buildVisitKey(date: string, facilityId: number, providerId: number): string {
  return `${date}:${facilityId}:${providerId}`;
}

function buildProviderKey(date: string, facilityId: number): string {
  return `${date}:${facilityId}`;
}

function getMemoryMap(storageKey: string): MemoryMap {
  try {
    const stored = sessionStorage.getItem(storageKey);
    return stored ? JSON.parse(stored) : {};
  } catch {
    return {};
  }
}

function setMemoryMap(storageKey: string, map: MemoryMap): void {
  try {
    sessionStorage.setItem(storageKey, JSON.stringify(map));
  } catch {
    // Silently fail if sessionStorage is full or unavailable
  }
}

export function useVisitMemory() {
  const getVisitId = useCallback((
    date: string,
    facilityId: number | null,
    providerId: number | null
  ): number | null => {
    if (!facilityId || !providerId) return null;
    const map = getMemoryMap(VISIT_STORAGE_KEY);
    const key = buildVisitKey(date, facilityId, providerId);
    return map[key] ?? null;
  }, []);

  const setVisitId = useCallback((
    date: string,
    facilityId: number | null,
    providerId: number | null,
    visitId: number | null
  ): void => {
    if (!facilityId || !providerId || !visitId) return;
    const map = getMemoryMap(VISIT_STORAGE_KEY);
    const key = buildVisitKey(date, facilityId, providerId);
    map[key] = visitId;
    setMemoryMap(VISIT_STORAGE_KEY, map);
  }, []);

  const getProviderId = useCallback((
    date: string,
    facilityId: number | null
  ): number | null => {
    if (!facilityId) return null;
    const map = getMemoryMap(PROVIDER_STORAGE_KEY);
    const key = buildProviderKey(date, facilityId);
    return map[key] ?? null;
  }, []);

  const setProviderId = useCallback((
    date: string,
    facilityId: number | null,
    providerId: number | null
  ): void => {
    if (!facilityId || !providerId) return;
    const map = getMemoryMap(PROVIDER_STORAGE_KEY);
    const key = buildProviderKey(date, facilityId);
    map[key] = providerId;
    setMemoryMap(PROVIDER_STORAGE_KEY, map);
  }, []);

  return { getVisitId, setVisitId, getProviderId, setProviderId };
}
