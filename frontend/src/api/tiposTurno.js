import apiClient from "./client";

export async function getTiposTurno() {
  const { data } = await apiClient.get("/tipos-turno");
  return data;
}

export async function createTipoTurno(payload) {
  const { data } = await apiClient.post("/tipos-turno", payload);
  return data;
}

export async function updateTipoTurno(id, payload) {
  const { data } = await apiClient.put(`/tipos-turno/${id}`, payload);
  return data;
}

export async function deleteTipoTurno(id) {
  await apiClient.delete(`/tipos-turno/${id}`);
}
