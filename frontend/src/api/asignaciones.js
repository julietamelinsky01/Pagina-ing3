import apiClient from "./client";

export async function getAsignaciones(desde, hasta) {
  const { data } = await apiClient.get("/asignaciones", { params: { desde, hasta } });
  return data;
}

export async function createAsignacion(payload) {
  const { data } = await apiClient.post("/asignaciones", payload);
  return data;
}

export async function deleteAsignacion(id) {
  await apiClient.delete(`/asignaciones/${id}`);
}
