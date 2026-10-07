import { useEffect, useState } from "react";
import {
  Box,
  Typography,
  TextField,
  Stack,
  Paper,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Alert,
  Button,
} from "@mui/material";
import { getAsignaciones } from "../api/asignaciones";
import { aISO, lunesDeLaSemana, sumarDias } from "../utils/fechas";
import { cargarFilasReporte } from "../utils/reglas";

export default function ReporteSemanal() {
  const inicioSemana = aISO(lunesDeLaSemana(aISO(new Date())));
  const [desde, setDesde] = useState(inicioSemana);
  const [hasta, setHasta] = useState(sumarDias(inicioSemana, 6));
  const [filas, setFilas] = useState([]);
  const [error, setError] = useState("");

  async function cargar() {
    setError("");
    try {
      setFilas(await cargarFilasReporte(desde, hasta, getAsignaciones));
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => {
    if (desde && hasta && desde <= hasta) {
      cargar();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [desde, hasta]);

  function exportarCsv() {
    const encabezado = "Empleado,Cantidad de turnos,Horas totales\n";
    const filasCsv = filas.map((f) => `${f.empleado},${f.turnos},${f.horas}`).join("\n");
    const blob = new Blob([encabezado + filasCsv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `reporte_${desde}_${hasta}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>Reporte semanal de horas</Typography>

      <Stack direction="row" spacing={2} sx={{ mb: 2, alignItems: "center" }}>
        <TextField
          label="Desde"
          type="date"
          value={desde}
          onChange={(e) => setDesde(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          label="Hasta"
          type="date"
          value={hasta}
          onChange={(e) => setHasta(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <Button
          variant="outlined"
          onClick={() => {
            setDesde(inicioSemana);
            setHasta(sumarDias(inicioSemana, 6));
          }}
        >
          Semana actual
        </Button>
        <Button variant="contained" onClick={exportarCsv} disabled={filas.length === 0}>
          Exportar CSV
        </Button>
      </Stack>

      {desde > hasta && <Alert severity="warning" sx={{ mb: 2 }}>"Hasta" no puede ser anterior a "Desde".</Alert>}
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Empleado</TableCell>
              <TableCell align="right">Cantidad de turnos</TableCell>
              <TableCell align="right">Horas totales</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filas.map((f) => (
              <TableRow key={f.empleadoId}>
                <TableCell>{f.empleado}</TableCell>
                <TableCell align="right">{f.turnos}</TableCell>
                <TableCell align="right">{f.horas}</TableCell>
              </TableRow>
            ))}
            {filas.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} align="center">
                  No hay turnos asignados en el rango seleccionado.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  );
}
