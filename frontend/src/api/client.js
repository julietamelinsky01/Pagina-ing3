import axios from "axios";

const apiClient = axios.create({
  // En producción (imagen) no hay ninguna dirección escrita: se pide a /api, del mismo origen,
  // y nginx lo reenvía al backend de ESE entorno (BACKEND_URL). VITE_API_URL queda sólo
  // para desarrollo local (`npm run dev`, ver .env.example).
  baseURL: import.meta.env.VITE_API_URL || "/api",
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("username");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    const mensaje =
      error.response?.data?.mensaje ||
      error.response?.data?.title ||
      "Ocurrió un error inesperado. Intentá de nuevo.";
    return Promise.reject(new Error(mensaje));
  }
);

export default apiClient;
