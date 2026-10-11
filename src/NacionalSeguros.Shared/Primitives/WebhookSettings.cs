namespace NacionalSeguros.Shared.Primitives;

public static class WebhookSettings
{
    public static string AresResumidorUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/ares-resumidor";
    public static string AresGeneradorPdfUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf-perfil-estructurado";
    public static string EnviarAreaWebhookUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/enviar_area";
    public static string PerfilVacanteResumenUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante_resumen";
    public static string PdfResumenEjecutivoUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/A4_Genera_PDF_resumen_ejecutivo_del_rol";
    public static string NotificadorSolicitanteUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/Notificador2_Solicitante_observa_perfil";
    public static string MotorMatchingUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/motor_matching";
    public static string MotorMatchingMvpUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/motor_Matching_Modificado_MVP";
    public static string AresGeneradorPdfSolicitudUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf";
    public static string PerfilVacanteWebhookUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante";
    public static string NotificadorDecisionRrhhUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/Notificador_Aprobado_Rechazado_Observado_SIR";
}
