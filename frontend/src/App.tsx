import { useEffect, useState } from "react";
import api from "./services/api";

interface HealthResponse {
  status: string;
  application: string;
  version: string;
}

function App() {
  const [health, setHealth] = useState<HealthResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkApi = async () => {
      try {
        const response = await api.get<HealthResponse>("/api/health");

        setHealth(response.data);
        setError(null);
      } catch {
        setError("Unable to connect to StockFlow API.");
      } finally {
        setLoading(false);
      }
    };

    checkApi();
  }, []);

  return (
    <main className="min-h-screen bg-slate-950 text-white p-8">
      <div className="max-w-5xl mx-auto">
        <header className="mb-12">
          <p className="text-blue-400 font-semibold tracking-widest text-sm">
            STOCKFLOW / INVENTORY MANAGEMENT
          </p>

          <h1 className="text-4xl font-bold mt-3">
            Welcome to StockFlow
          </h1>

          <p className="text-slate-400 mt-3">
            Your inventory management workspace.
          </p>
        </header>

        <section className="grid gap-5 md:grid-cols-2">
          <div className="rounded-xl border border-slate-800 bg-slate-900 p-6">
            <h2 className="text-lg font-semibold mb-4">
              Backend Connection
            </h2>

            {loading ? (
              <p className="text-slate-400">Checking API...</p>
            ) : error ? (
              <p className="text-red-400">{error}</p>
            ) : (
              <div>
                <p className="text-green-400 font-semibold">
                  ● Connected
                </p>

                <p className="text-slate-400 mt-2">
                  {health?.application}
                </p>

                <p className="text-slate-500 text-sm mt-1">
                  Version {health?.version}
                </p>
              </div>
            )}
          </div>

          <div className="rounded-xl border border-slate-800 bg-slate-900 p-6">
            <h2 className="text-lg font-semibold mb-4">
              Project Status
            </h2>

            <p className="text-blue-400 font-semibold">
              Milestone 1 — Foundation
            </p>

            <p className="text-slate-400 mt-2">
              Backend and frontend foundation is being established.
            </p>
          </div>
        </section>
      </div>
    </main>
  );
}

export default App;
