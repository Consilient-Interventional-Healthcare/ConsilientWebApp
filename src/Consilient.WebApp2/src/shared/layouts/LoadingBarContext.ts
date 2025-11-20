import { createContext } from "react";

export interface LoadingBarApi {
  start: () => void;
  complete: () => void;
}

const LoadingBarContext = createContext<LoadingBarApi | null>(null);
export default LoadingBarContext;
