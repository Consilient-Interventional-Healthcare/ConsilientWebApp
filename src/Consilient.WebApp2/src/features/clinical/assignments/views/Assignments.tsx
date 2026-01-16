import { useParams } from 'react-router-dom';

export default function Assignments() {
  const { id } = useParams<{ id: string }>();

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Assignment</h1>
        <p className="text-gray-600">ID: {id}</p>
      </div>
    </div>
  );
}
