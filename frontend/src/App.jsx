import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import RutaProtegida from "./components/RutaProtegida";
import Layout from "./components/Layout";
import Login from "./pages/Login";
import Empleados from "./pages/Empleados";
import Calendario from "./pages/Calendario";
import ReporteSemanal from "./pages/ReporteSemanal";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            element={
              <RutaProtegida>
                <Layout />
              </RutaProtegida>
            }
          >
            <Route path="/empleados" element={<Empleados />} />
            <Route path="/calendario" element={<Calendario />} />
            <Route path="/reporte" element={<ReporteSemanal />} />
          </Route>
          <Route path="*" element={<Navigate to="/empleados" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
