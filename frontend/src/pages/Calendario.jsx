import { useEffect, useState } from "react";
import {
  Box,
  Typography,
  Button,
  Paper,
  Chip,
  IconButton,
  Alert,
  Stack,
} from "@mui/material";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import CloseIcon from "@mui/icons-material/Close";
import AddIcon from "@mui/icons-material/Add";
import { getAsignaciones, createAsignacion, deleteAsignacion } from "../api/asignaciones";
import { getEmpleados } from "../api/empleados";
import { getTiposTurno } from "../api/tiposTurno";
import { aISO, lunesDeLaSemana, diasDeLaSemana, sumarDias, NOMBRES_DIA } from "../utils/fechas";
import AsignacionForm from "./AsignacionForm";

export default function Calendario() {
  const [lunes, setLunes] = useState(() => aISO(lunesDeLaSemana(aISO(new Date()))));
  const [asignaciones, setAsignaciones] = useState([]);
  const [empleados, setEmpleados] = useState([]);
  const [tiposTurno, setTiposTurno] = useState([]);
  const [error, setError] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [fechaSeleccionada, setFechaSeleccionada] = useState(null);

  const dias = diasDeLaSemana(lunesDeLaSemana(lunes));
  const domingo = sumarDias(lunes, 6);

  async function cargarSemana() {
    setError("");
    try {
      const [asigs, emps, tipos] = await Promise.all([
        getAsignaciones(lunes, domingo),
        getEmpleados(true),
        getTiposTurno(),
      ]);
      setAsignaciones(asigs);
      setEmpleados(emps);
      setTiposTurno(tipos);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => {
    cargarSemana();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lunes]);

  function abrirForm(fechaISO) {
    setFechaSeleccionada(fechaISO);
    setFormOpen(true);
  }

  async function handleSubmit(payload) {
    await createAsignacion(payload);
    setFormOpen(false);
    await cargarSemana();
  }

  async function handleEliminar(id) {
    if (!window.confirm("¿Eliminar esta asignación?")) return;
    try {
      await deleteAsignacion(id);
      await cargarSemana();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
        <Typography variant="h5" sx={{ fontWeight: 700 }}>Calendario de turnos</Typography>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <IconButton onClick={() => setLunes(sumarDias(lunes, -7))}>
            <ChevronLeftIcon />
          </IconButton>
          <Typography variant="body1">
            {lunes} — {domingo}
          </Typography>
          <IconButton onClick={() => setLunes(sumarDias(lunes, 7))}>
            <ChevronRightIcon />
          </IconButton>
        </Stack>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: "repeat(7, minmax(150px, 1fr))",
          gap: 1,
          overflowX: "auto",
        }}
      >
        {dias.map((dia, idx) => {
          const fechaISO = aISO(dia);
          const asignacionesDia = asignaciones.filter((a) => a.fecha === fechaISO);
          return (
            <Paper key={fechaISO} sx={{ p: 1, minHeight: 220, bgcolor: "#fff" }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
                {NOMBRES_DIA[idx]}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {fechaISO}
              </Typography>
              <Stack spacing={0.5} sx={{ mt: 1, mb: 1 }}>
                {asignacionesDia.map((a) => (
                  <Chip
                    key={a.id}
                    label={`${a.tipoTurnoNombre}: ${a.empleadoNombreCompleto}`}
                    size="small"
                    onDelete={() => handleEliminar(a.id)}
                    deleteIcon={<CloseIcon />}
                    sx={{ justifyContent: "space-between" }}
                  />
                ))}
                {asignacionesDia.length === 0 && (
                  <Typography variant="caption" color="text.secondary">
                    Sin turnos
                  </Typography>
                )}
              </Stack>
              <Button size="small" startIcon={<AddIcon />} onClick={() => abrirForm(fechaISO)}>
                Agregar
              </Button>
            </Paper>
          );
        })}
      </Box>

      {formOpen && (
        <AsignacionForm
          open={formOpen}
          fecha={fechaSeleccionada}
          empleados={empleados}
          tiposTurno={tiposTurno}
          asignaciones={asignaciones}
          onClose={() => setFormOpen(false)}
          onSubmit={handleSubmit}
        />
      )}
    </Box>
  );
}
