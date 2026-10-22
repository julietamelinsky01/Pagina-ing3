# infra/ — el entorno `preprod`

Declara un tercer entorno (entre QA y PROD) con Terraform: un proyecto de Neon con su base, y dos
servicios de Render que ejecutan la imagen del TP7. **No toca QA ni PROD**: no los declara.

## Cómo se crea y se destruye

```bash
export NEON_API_KEY=...          # Neon → Account settings → API keys
export RENDER_API_KEY=...        # Render → Account Settings → API Keys
export RENDER_OWNER_ID=tea-...   # curl -s -H "Authorization: Bearer $RENDER_API_KEY" https://api.render.com/v1/owners
export TF_VAR_neon_org_id=org-... # curl -s -H "Authorization: Bearer $NEON_API_KEY" https://console.neon.tech/api/v2/users/me/organizations

cd infra
terraform init      # baja los providers fijados por versión (.terraform.lock.hcl)
terraform plan      # qué va a pasar, sin hacerlo
terraform apply     # crea preprod
terraform destroy   # lo borra entero (hacelo al terminar: 6 servicios gratuitos comparten las 750 h)
```

Las claves no se escriben en ningún archivo: los providers las leen del entorno.
El **estado** (`terraform.tfstate`) vive en la máquina de quien aplica y **no se commitea**:
guarda contraseñas en texto plano. Si se pierde con preprod vivo, hay que importar los recursos.

## Qué se deriva (no se copia)

- La cadena de conexión de la api se arma con lo que devuelve Neon (`local.conn`).
- `BACKEND_URL` del front sale de la `url` de la api.
- La clave JWT la genera Terraform (`random_password`).
