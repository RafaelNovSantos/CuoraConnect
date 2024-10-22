using Microsoft.AspNetCore.Components;

public class NavigationInterceptor
{
    private readonly NavigationManager _navigationManager;

    public NavigationInterceptor(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public bool IsNavigatingAway { get; private set; }
    public string CurrentPage => _navigationManager.Uri; // Propriedade para obter a URL atual

    public void SetNavigatingAway(bool isNavigating)
    {
        IsNavigatingAway = isNavigating;
    }

    public async Task<bool> InterceptNavigationAsync(Func<Task> action)
    {
        if (IsNavigatingAway)
            return false;

        await action();

        return !IsNavigatingAway; // Retorna true se não houver navegação
    }
}
