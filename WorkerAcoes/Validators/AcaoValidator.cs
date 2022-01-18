using FluentValidation;
using WorkerAcoes.Models;

namespace WorkerAcoes.Validators;

public class AcaoValidator : AbstractValidator<Acao>
{
    public AcaoValidator()
    {
        RuleFor(c => c.Codigo).NotEmpty().WithMessage("Preencha o campo 'Codigo'")
            .MinimumLength(4).WithMessage("O campo 'Codigo' deve possuir no mínimo 4 caracteres")
            .MaximumLength(10).WithMessage("O campo 'Codigo' deve possuir no máximo 10 caracteres");

        RuleFor(c => c.Valor).NotEmpty().WithMessage("Preencha o campo 'Valor'")
            .GreaterThan(0).WithMessage("O campo 'Valor' deve ser maior do 0");

        RuleFor(c => c.CodCorretora).NotEmpty().WithMessage("Preencha o campo 'CodCorretora'")
            .MinimumLength(4).WithMessage("O campo 'CodCorretora' deve possuir no mínimo 4 caracteres")
            .MaximumLength(10).WithMessage("O campo 'CodCorretora' deve possuir no máximo 10 caracteres");

        RuleFor(c => c.NomeCorretora).NotEmpty().WithMessage("Preencha o campo 'NomeCorretora'")
            .MinimumLength(4).WithMessage("O campo 'NomeCorretora' deve possuir no mínimo 4 caracteres")
            .MaximumLength(60).WithMessage("O campo 'NomeCorretora' deve possuir no máximo 60 caracteres");
    }
}