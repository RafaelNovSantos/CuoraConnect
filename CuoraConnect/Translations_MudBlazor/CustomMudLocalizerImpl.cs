using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Localization;
using MudBlazor;

internal class DictionaryMudLocalizer : MudLocalizer
{
    private Dictionary<string, string> _localization;

    public DictionaryMudLocalizer()
    {
        _localization = new()
        {
            {"MudStepper_Next", "Próximo" },
            {"MudStepper_Previous", "Voltar" },
            {"MudStepper_Reset", "Resetar"},
            {"MudDataGridPager_InfoFormat", "{0}-{1} de {2}"},
            {"MudStepper_Complete", "Completar"},
        };
    }

    public override LocalizedString this[string key]
    {
        get
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Parent.TwoLetterISOLanguageName;
            if (currentCulture.Equals("pt", StringComparison.InvariantCultureIgnoreCase)
                && _localization.TryGetValue(key, out var res))
            {
                return new(key, res);
            }
            else
            {
                return new(key, key, true);
            }
        }
    }
}