import { AppBar, Toolbar, Typography, Button, Box, Container } from "@mui/material";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const navLinkStyle = ({ isActive }) => ({
  color: "white",
  textDecoration: "none",
  marginRight: 20,
  fontWeight: isActive ? 700 : 400,
  borderBottom: isActive ? "2px solid white" : "2px solid transparent",
  paddingBottom: 4,
});

export default function Layout() {
  const { username, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#f5f2ee" }}>
      <AppBar position="static" sx={{ bgcolor: "#5c3a21" }}>
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 0, mr: 4, fontWeight: 700 }}>
            Las Melis
          </Typography>
          <Box sx={{ flexGrow: 1, display: "flex" }}>
            <NavLink to="/empleados" style={navLinkStyle}>Empleados</NavLink>
            <NavLink to="/calendario" style={navLinkStyle}>Calendario</NavLink>
            <NavLink to="/reporte" style={navLinkStyle}>Reporte semanal</NavLink>
          </Box>
          <Typography variant="body2" sx={{ mr: 2 }}>{username}</Typography>
          <Button color="inherit" onClick={handleLogout}>Salir</Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ py: 4 }}>
        <Outlet />
      </Container>
    </Box>
  );
}
