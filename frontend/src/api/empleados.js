import apiClient from "./client";

export async function getEmpleados(activo) {
  const params = {};
  if (activo !== undefined && activo !== null) params.activo = activo;
  const { data } = await apiClient.get("/empleados", { params });
  return data;
}

export async function createEmpleado(payload) {
  const { data } = await apiClient.post("/empleados", payload);
  return data;
}

export async function updateEmpleado(id, payload) {
  const { data } = await apiClient.put(`/empleados/${id}`, payload);
  return data;
}

export async function bajaEmpleado(id) {
  const { data } = await apiClient.delete(`/empleados/${id}`);
  return data;
}
