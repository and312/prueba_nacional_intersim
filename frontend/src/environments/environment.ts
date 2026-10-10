export const environment = {
  production: true,
  apiUrl: '/api/v1',
  webhooks: {
    generateExecutiveSummaryPdf: 'https://nacional-seguros-dev.isia.cloud/webhook/A4_Genera_PDF_resumen_ejecutivo_del_rol',
    generateStructuredProfilePdf: 'https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf-perfil-estructurado',
    generarResumen: 'https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante_resumen',
    notifySolicitanteProfileUnderReview: 'https://nacional-seguros-dev.isia.cloud/webhook/Notificador2_Solicitante_observa_perfil',
    triggerMotorMatching: 'https://nacional-seguros-dev.isia.cloud/webhook/motor_matching',
    triggerMotorMatchingModificadoMVP: 'https://nacional-seguros-dev.isia.cloud/webhook/motor_Matching_Modificado_MVP'
  }
};

