import { useEffect, useState } from "react";
import {
  Box,
  Typography,
  Button,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Paper,
  Chip,
  ToggleButtonGroup,
  ToggleButton,
  Alert,
  Snackbar,
} from "@mui/material";
import { getEmpleados, createEmpleado, updateEmpleado, bajaEmpleado } from "../api/empleados";
import EmpleadoForm from "./EmpleadoForm";

export default function Empleados() {
  const [empleados, setEmpleados] = useState([]);
  const [filtro, setFiltro] = useState("activos");
  const [formOpen, setFormOpen] = useState(false);
  const [empleadoEditando, setEmpleadoEditando] = useState(null);
  const [error, setError] = useState("");
  const [aviso, setAviso] = useState("");

  async function cargar() {
    setError("");
    try {
      const activo = filtro === "activos" ? true : filtro === "inactivos" ? false : undefined;
      const data = await getEmpleados(activo);
      setEmpleados(data);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filtro]);

  function abrirNuevo() {
    setEmpleadoEditando(null);
    setFormOpen(true);
  }

  function abrirEditar(empleado) {
    setEmpleadoEditando(empleado);
    setFormOpen(true);
  }

  async function handleSubmit(form) {
    const payload = {
      ...form,
      telefono: form.telefono.trim() === "" ? null : form.telefono.trim(),
      email: form.email.trim() === "" ? null : form.email.trim(),
    };
    if (empleadoEditando) {
      await updateEmpleado(empleadoEditando.id, payload);
    } else {
      await createEmpleado(payload);
    }
    setFormOpen(false);
    await cargar();
  }

  async function handleBaja(empleado) {
    if (!window.confirm(`¿Dar de baja a ${empleado.nombre} ${empleado.apellido}?`)) return;
    try {
      const resultado = await bajaEmpleado(empleado.id);
      setAviso(resultado.mensaje);
      await cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
        <Typography variant="h5" sx={{ fontWeight: 700 }}>Empleados</Typography>
        <Button variant="contained" onClick={abrirNuevo}>Nuevo empleado</Button>
      </Box>

      <ToggleButtonGroup
        value={filtro}
        exclusive
        onChange={(e, val) => val && setFiltro(val)}
        size="small"
        sx={{ mb: 2 }}
      >
        <ToggleButton value="activos">Activos</ToggleButton>
        <ToggleButton value="inactivos">Inactivos</ToggleButton>
        <ToggleButton value="todos">Todos</ToggleButton>
      </ToggleButtonGroup>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nombre</TableCell>
              <TableCell>Apellido</TableCell>
              <TableCell>DNI</TableCell>
              <TableCell>Teléfono</TableCell>
              <TableCell>Estado</TableCell>
              <TableCell align="right">Acciones</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {empleados.map((emp) => (
              <TableRow key={emp.id}>
                <TableCell>{emp.nombre}</TableCell>
                <TableCell>{emp.apellido}</TableCell>
                <TableCell>{emp.dni}</TableCell>
                <TableCell>{emp.telefono || "-"}</TableCell>
                <TableCell>
                  <Chip
                    label={emp.activo ? "Activo" : "Inactivo"}
                    color={emp.activo ? "success" : "default"}
                    size="small"
                  />
                </TableCell>
                <TableCell align="right">
                  <Button size="small" onClick={() => abrirEditar(emp)}>Editar</Button>
                  {emp.activo && (
                    <Button size="small" color="error" onClick={() => handleBaja(emp)}>
                      Dar de baja
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))}
            {empleados.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  No hay empleados para mostrar.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>

      <EmpleadoForm
        open={formOpen}
        empleado={empleadoEditando}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
      />

      <Snackbar
        open={Boolean(aviso)}
        autoHideDuration={5000}
        onClose={() => setAviso("")}
        message={aviso}
      />
    </Box>
  );
}
