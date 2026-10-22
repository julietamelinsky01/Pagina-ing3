# PREPROD: un entorno entre QA y producción que NO existe hasta que este código lo crea.
# QA y PROD (creados a mano en el TP6) no se declaran, no se importan y no se tocan.

terraform {
  required_version = ">= 1.5"

  required_providers {
    # Fijados por versión: sin esto, un día `terraform init` baja otra cosa y el archivo deja de compilar.
    neon = {
      source  = "kislerdm/neon"
      version = "0.18.0"
    }
    render = {
      source  = "render-oss/render"
      version = "1.9.1"
    }
    random = {
      source  = "hashicorp/random"
      version = "3.8.1"
    }
  }
}

provider "neon" {}   # lee NEON_API_KEY del entorno: ninguna credencial escrita acá
provider "render" {} # lee RENDER_API_KEY y RENDER_OWNER_ID del entorno

# --- La base de datos: un proyecto de Neon propio, que se puede destruir entero sin rozar QA ni PROD ---

resource "neon_project" "preprod" {
  name   = "pagina-ing3-preprod"
  org_id = var.neon_org_id # Neon agrupa los proyectos por organización y la API lo exige

  # El plan gratuito de Neon topea el historial en 6 horas; sin esta línea el primer apply falla.
  history_retention_seconds = 21600
}

resource "neon_role" "preprod" {
  project_id = neon_project.preprod.id
  branch_id  = neon_project.preprod.default_branch_id
  name       = "app_preprod_owner"
}

resource "neon_database" "preprod" {
  project_id = neon_project.preprod.id
  branch_id  = neon_project.preprod.default_branch_id
  name       = "app_preprod"
  owner_name = neon_role.preprod.name
}

# --- La clave JWT de preprod: se genera acá, vive sólo en el estado y no se escribe en ningún archivo ---

resource "random_password" "jwt_key" {
  length  = 48
  special = false
}

locals {
  # La cadena de conexión se ARMA con lo que devolvió Neon. Nadie la copia y pega.
  conn = "Host=${neon_project.preprod.database_host};Database=${neon_database.preprod.name};Username=${neon_role.preprod.name};Password=${neon_role.preprod.password};SSL Mode=Require"
}

# --- Los dos servicios: ejecutan la imagen del TP7, no reconstruyen nada ---

resource "render_web_service" "api" {
  name   = "pagina-ing3-api-preprod"
  plan   = "free"
  region = var.region

  runtime_source = {
    image = {
      image_url = var.image_repo_api # SIN la etiqueta pegada
      tag       = var.image_tag      # la etiqueta va en su propio campo
    }
  }

  # Mismos nombres de variables que usan QA y PROD (TP6): cambian los valores, no la imagen.
  # No se declara APP_COMMIT ni Frontend__Origin: el front llama a la api por el proxy de nginx (mismo origen).
  env_vars = {
    ConnectionStrings__DefaultConnection = { value = local.conn }
    Jwt__Key                             = { value = random_password.jwt_key.result }
    Jwt__Issuer                          = { value = "LasMelisApi" }
    Jwt__Audience                        = { value = "LasMelisClient" }
  }
}

resource "render_web_service" "front" {
  name   = "pagina-ing3-front-preprod"
  plan   = "free"
  region = var.region

  runtime_source = {
    image = {
      image_url = var.image_repo_front
      tag       = var.image_tag
    }
  }

  env_vars = {
    BACKEND_URL  = { value = render_web_service.api.url } # la URL sale de Terraform, no de un copy-paste
    DNS_RESOLVER = { value = "8.8.8.8" }
  }
}
