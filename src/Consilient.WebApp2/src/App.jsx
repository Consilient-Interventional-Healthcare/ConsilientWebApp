import './App.css'
import './index.css'
import FileUpload from './components/FileUpload'
import { FluidLayout } from './layouts/FluidLayout'
import { Outlet, useMatches } from "react-router-dom";
import { useEffect } from "react";

function App() {
  const matches = useMatches();
  
  useEffect(() => {
    const currentMatch = matches[matches.length - 1];
    const title = currentMatch?.handle?.title ?? currentMatch?.handle?.label;
    
    if (title) {
      document.title = `${title} - Consilient`;
    } else {
      document.title = 'Consilient';
    }
  }, [matches]);

  return (
    <FluidLayout>
      <div className="px-4 lg:px-8 py-6">
        <Outlet />
      </div>
    </FluidLayout>
  )
}

export default App;
