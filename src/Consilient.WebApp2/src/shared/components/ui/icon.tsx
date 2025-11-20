import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import * as solidIcons from "@fortawesome/free-solid-svg-icons";
import type { IconDefinition } from "@fortawesome/fontawesome-svg-core";

interface IconProps {
  name: string;
  className?: string;
  ariaHidden?: boolean;
}

export const Icon: React.FC<IconProps> = ({ name, className = "", ariaHidden = true }) => {
  // Convert name to FontAwesome PascalCase (e.g., "home" -> "faHome")
  const pascal = "fa" + name
    .split("-")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join("");
  const faIcon = (solidIcons as unknown as Record<string, IconDefinition>)[pascal];

  if (faIcon?.iconName) {
    return <FontAwesomeIcon icon={faIcon} className={className} aria-hidden={ariaHidden} />;
  }
  // Fallback if icon not found
  return <span className={className} aria-hidden={ariaHidden}>❓</span>;
};
