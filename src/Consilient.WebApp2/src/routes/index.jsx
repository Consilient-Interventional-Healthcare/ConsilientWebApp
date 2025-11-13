import { createBrowserRouter } from "react-router-dom";
import App from "@/App";

// Import your page components here
import Dashboard from "@/views/dashboard";
import ClinicalAssignmentsImport from "@/views/clinical/assignments/Import";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        index: true,
        element: <Dashboard />,
        handle: { label: "Dashboard", title: "Dashboard" },
      },
      {
        path: "clinical",
        handle: { 
          label: "Clinical Management",
          title: "Clinical Management",
        },
        children: [
          {
            index: true,
            element: <div>Clinical Management Overview</div>,
          },
          {
            path: "assignments",
            children: [
              {
                path: "import",
                element: <ClinicalAssignmentsImport />,
                handle: { 
                  label: "Import Assignments",
                  title: "Import Assignments",
                },
              },
            ],
          },
        ],
      },
      {
        path: "billing",
        element: <div>Billing</div>,
        handle: { 
          label: "Billing",
          title: "Billing",
        },
      },
    ],
  },
]);

// Generate navItems from router with subnav
export const navItems = router.routes[0].children
  .filter(route => route.handle?.label)
  .map(route => {
    const subNav = [];
    
    // Extract subnav from nested children
    if (route.children) {
      const extractSubNav = (children, parentPath = '') => {
        children.forEach(child => {
          const fullPath = route.index ? "/" : `/${route.path}${parentPath}${child.path ? '/' + child.path : ''}`;
          
          if (child.handle?.label && child.path) {
            subNav.push({
              href: fullPath,
              label: child.handle.label,
            });
          }
          
          if (child.children) {
            extractSubNav(child.children, `${parentPath}${child.path ? '/' + child.path : ''}`);
          }
        });
      };
      
      extractSubNav(route.children);
    }
    
    return {
      href: route.index ? "/" : `/${route.path}`,
      label: route.handle.label,
      subNav,
    };
  });