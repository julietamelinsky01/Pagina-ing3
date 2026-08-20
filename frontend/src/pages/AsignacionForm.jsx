import { useState, useMemo } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  MenuItem,
  Alert,
} from "@mui/material";

export default function AsignacionForm({ open, fecha, empleados, tiposTurno, asignaciones, onClose, onSubmit }) {
  const [empleadoId, setEmpleadoId] = useState("");
  const [tipoTurnoId, setTipoTurnoId] = useState("");
  const [observaciones, setObservaciones] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const empleadosActivos = useMemo(() => empleados.filter((e) => e.activo), [empleados]);

  // Chequeo del lado del cliente antes de pegarle a la API: evita el viaje redondo
  // para el caso más común de duplicado (mismo empleado + turno + fecha ya visibles en la semana).
  const yaExiste = useMemo(() => {
    if (!empleadoId || !tipoTurnoId) return false;
    return asignaciones.some(
      (a) =>
        String(a.empleadoId) === String(empleadoId) &&
        String(a.tipoTurnoId) === String(tipoTurnoId) &&
        a.fecha === fecha
    );
  }, [asignaciones, empleadoId, tipoTurnoId, fecha]);

  function reset() {
    setEmpleadoId("");
    setTipoTurnoId("");
    setObservaciones("");
    setError("");
  }

  function handleClose() {
    reset();
    onClose();
  }

  async function handleSubmit(e) {
    e.preventDefault();
    if (!empleadoId || !tipoTurnoId || yaExiste) return;
    setError("");
    setSaving(true);
    try {
      await onSubmit({ empleadoId: Number(empleadoId), tipoTurnoId: Number(tipoTurnoId), fecha, observaciones });
      reset();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="xs">
      <DialogTitle>Nueva asignación — {fecha}</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          {yaExiste && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Ese empleado ya tiene ese turno asignado este día.
            </Alert>
          )}
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              label="Empleado"
              value={empleadoId}
              onChange={(e) => setEmpleadoId(e.target.value)}
              required
              fullWidth
            >
              {empleadosActivos.map((e) => (
                <MenuItem key={e.id} value={e.id}>
                  {e.nombre} {e.apellido}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label="Tipo de turno"
              value={tipoTurnoId}
              onChange={(e) => setTipoTurnoId(e.target.value)}
              required
              fullWidth
            >
              {tiposTurno.map((t) => (
                <MenuItem key={t.id} value={t.id}>
                  {t.nombre} ({t.horaInicio.slice(0, 5)}–{t.horaFin.slice(0, 5)})
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Observaciones"
              value={observaciones}
              onChange={(e) => setObservaciones(e.target.value)}
              fullWidth
              multiline
              minRows={2}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleClose}>Cancelar</Button>
          <Button type="submit" variant="contained" disabled={!empleadoId || !tipoTurnoId || yaExiste || saving}>
            {saving ? "Guardando..." : "Asignar"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
