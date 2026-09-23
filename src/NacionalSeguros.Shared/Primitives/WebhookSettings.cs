namespace NacionalSeguros.Shared.Primitives;

public static class WebhookSettings
{
    public static string AresResumidorUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/ares-resumidor";
    public static string EnviarAreaWebhookUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/enviar_area";
    public static string PerfilVacanteResumenUrl { get; set; } = "https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante_resumen";
}
