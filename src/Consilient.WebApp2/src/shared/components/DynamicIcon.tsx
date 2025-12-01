import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { library } from '@fortawesome/fontawesome-svg-core';
import { fas } from '@fortawesome/free-solid-svg-icons';
import type { CSSProperties } from 'react';

type CSSVariables = Record<`--fa-font-${string}`, string>;

library.add(fas);

import type { IconName } from '@fortawesome/fontawesome-svg-core';

interface DynamicIconProps {
  iconName: string;
  className?: string;
  style?: (CSSProperties & CSSVariables) | undefined;
}

export function DynamicIcon({ iconName, className, style }: DynamicIconProps) {
  if (!iconName) return null; // Prevent error if iconName is missing
  return (
    <FontAwesomeIcon
      icon={['fas', iconName as IconName]}
      className={className}
      style={style}
    />
  );
}
