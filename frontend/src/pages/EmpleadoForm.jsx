import { useEffect, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  Alert,
} from "@mui/material";

const vacio = { nombre: "", apellido: "", dni: "", telefono: "", email: "", fechaIngreso: "" };

export default function EmpleadoForm({ open, empleado, onClose, onSubmit }) {
  const [form, setForm] = useState(vacio);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setError("");
      setForm(
        empleado
          ? {
              nombre: empleado.nombre,
              apellido: empleado.apellido,
              dni: empleado.dni,
              telefono: empleado.telefono || "",
              email: empleado.email || "",
              fechaIngreso: empleado.fechaIngreso,
            }
          : vacio
      );
    }
  }, [open, empleado]);

  const dniValido = /^\d{7,8}$/.test(form.dni);
  const camposCompletos =
    form.nombre.trim() !== "" &&
    form.apellido.trim() !== "" &&
    dniValido &&
    form.fechaIngreso !== "";

  async function handleSubmit(e) {
    e.preventDefault();
    if (!camposCompletos) return;
    setError("");
    setSaving(true);
    try {
      await onSubmit(form);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{empleado ? "Editar empleado" : "Nuevo empleado"}</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Nombre"
              value={form.nombre}
              onChange={(e) => setForm({ ...form, nombre: e.target.value })}
              required
              fullWidth
            />
            <TextField
              label="Apellido"
              value={form.apellido}
              onChange={(e) => setForm({ ...form, apellido: e.target.value })}
              required
              fullWidth
            />
            <TextField
              label="DNI"
              value={form.dni}
              onChange={(e) => setForm({ ...form, dni: e.target.value })}
              required
              fullWidth
              error={form.dni !== "" && !dniValido}
              helperText={
                form.dni !== "" && !dniValido ? "El DNI debe ser numérico, de 7 u 8 dígitos." : " "
              }
            />
            <TextField
              label="Teléfono"
              value={form.telefono}
              onChange={(e) => setForm({ ...form, telefono: e.target.value })}
              fullWidth
            />
            <TextField
              label="Email"
              type="email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              fullWidth
            />
            <TextField
              label="Fecha de ingreso"
              type="date"
              value={form.fechaIngreso}
              onChange={(e) => setForm({ ...form, fechaIngreso: e.target.value })}
              required
              fullWidth
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose}>Cancelar</Button>
          <Button type="submit" variant="contained" disabled={!camposCompletos || saving}>
            {saving ? "Guardando..." : "Guardar"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
