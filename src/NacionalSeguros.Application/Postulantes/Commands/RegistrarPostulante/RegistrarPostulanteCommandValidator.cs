using System.IO;
using FluentValidation;

namespace NacionalSeguros.Application.Postulantes.Commands.RegistrarPostulante;

public class RegistrarPostulanteCommandValidator : AbstractValidator<RegistrarPostulanteCommand>
{
    public RegistrarPostulanteCommandValidator()
    {
        RuleFor(x => x.VacanteId)
            .GreaterThan(0).WithMessage("El ID de la vacante debe ser mayor a 0.");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("El formato del correo es inválido.");

        RuleFor(x => x.CvContent)
            .NotNull().WithMessage("El contenido del CV es requerido.")
            .Must(content => content.Length > 0).WithMessage("El archivo del CV no puede estar vacío.")
            .Must(content => content.Length <= 5 * 1024 * 1024).WithMessage("El archivo del CV no debe superar los 5 MB.");

        RuleFor(x => x.CvFileName)
            .NotEmpty().WithMessage("El nombre del archivo del CV es requerido.")
            .Must(fileName =>
            {
                var ext = Path.GetExtension(fileName).ToLower();
                return ext == ".pdf" || ext == ".docx";
            }).WithMessage("El archivo del CV debe ser en formato PDF (.pdf) o Word (.docx).");
    }
}
