#!/bin/bash
# 1. Crear solicitud vía vacancy-requests (n8n)
echo -e "\n=== 1. CREAR SOLICITUD (n8n) ==="
response=$(curl -s -k -X POST "https://localhost/api/internal/vacancy-requests" -H "Content-Type: application/json" -H "X-Internal-Api-Key: workflow_rrhh_secret_key_2026" -d '{
  "title": "Solicitud tecnica WhatsApp",
  "area": "Finanzas",
  "requestedByUsuarioId": "6",
  "reason": "Registro inicial desde n8n",
  "requestType": "NuevaPosicion",
  "vacancyCount": 1,
  "seniority": "Semi Senior",
  "mainFunctions": "Analisis de cartera",
  "requiredSkills": "Excel, SQL",
  "channel": "WHATSAPP",
  "decisionMaker": "Gerencia Finanzas",
  "priority": "Alta",
  "employmentType": "Planilla",
  "workMode": "Hibrido",
  "location": "La Paz",
  "comments": "Creada desde endpoint interno"
}')
echo "Response: $response"

vacancyRequestId=$(echo "$response" | grep -oP '"id":\s*\K[0-9]+')
vacancyRequestCode=$(echo "$response" | grep -oP '"requestCode":\s*"\K[^"]+')
echo "Vacancy Request ID: $vacancyRequestId"
echo "Vacancy Request Code: $vacancyRequestCode"

# 2. Obtener solicitud por ID (n8n)
echo -e "\n=== 2. OBTENER SOLICITUD POR ID (n8n) ==="
curl -k -s -X GET "https://localhost/api/internal/vacancy-requests/$vacancyRequestId" -H "X-Internal-Api-Key: workflow_rrhh_secret_key_2026"
echo ""

# 3. Listar solicitudes con filtros (n8n)
echo -e "\n=== 3. LISTAR SOLICITUDES CON FILTROS (n8n) ==="
curl -k -s -X GET "https://localhost/api/internal/vacancy-requests?area=Finanzas&requestedByUsuarioId=6&status=Recibida" -H "X-Internal-Api-Key: workflow_rrhh_secret_key_2026"
echo ""

# 4. Actualizar solicitud parcial (n8n)
echo -e "\n=== 4. ACTUALIZAR SOLICITUD PARCIAL (n8n) ==="
curl -k -s -X PATCH "https://localhost/api/internal/vacancy-requests/$vacancyRequestId" -H "Content-Type: application/json" -H "X-Internal-Api-Key: workflow_rrhh_secret_key_2026" -d '{
  "seniority": "Senior",
  "requiredSkills": "Excel, SQL, Power BI",
  "decisionMaker": "Gerencia Finanzas y RRHH",
  "comments": "Actualizada parcialmente desde n8n"
}'
echo ""

# 5. Obtener solicitud por código (n8n)
echo -e "\n=== 5. OBTENER SOLICITUD POR CODIGO (n8n) ==="
curl -k -s -X GET "https://localhost/api/internal/vacancy-requests/by-code/$vacancyRequestCode" -H "X-Internal-Api-Key: workflow_rrhh_secret_key_2026"
echo ""
