# Estos valores NO son secretos (el nombre de una imagen pública, una etiqueta, una región):
# van con default en un archivo que SÍ se commitea.

variable "image_repo_api" {
  type    = string
  default = "ghcr.io/julietamelinsky01/pagina-ing3-backend" # SIN la etiqueta
}

variable "image_repo_front" {
  type    = string
  default = "ghcr.io/julietamelinsky01/pagina-ing3-frontend" # SIN la etiqueta
}

# Preprod corre la imagen de un merge concreto (la release v7.0.0). Para otra, se cambia esta línea
# por PR y se recrea el servicio: un entorno desechable no se "actualiza", se vuelve a hacer.
variable "image_tag" {
  type    = string
  default = "sha-db433c2caf27958503963c1251e7c1c45e350819"
}

variable "region" {
  type    = string
  default = "oregon" # frankfurt | ohio | oregon | singapore | virginia
}

# El id de tu organización de Neon (Organization settings → General). No es un secreto, pero es
# de TU cuenta: no tiene default y se pasa por el entorno con `export TF_VAR_neon_org_id=org-…`.
variable "neon_org_id" {
  type = string
}
