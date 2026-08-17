using System;
using System.Collections.Generic;
using Base_Classes;
using UI_MVP.ChooseName;
using UI_MVP.ChooseName.UI_MVP.ChooseName;
using UI.InGame;
using UI.MainMenu;
using UnityEngine;

// Stable screen identity used by UIManager runtime navigation APIs.
public enum UIScreenId
{
    MainMenu,
    InGame,
    ChooseName
}

public enum UIPresenterType
{
    MainMenu,
    InGame,
    ChooseName
}

[Serializable]
public class UIPresenterBinding
{
    public UIScreenId ScreenId = UIScreenId.MainMenu;
    public UIPresenterType PresenterType = UIPresenterType.MainMenu;
    public ViewBase View;
    public bool ShowOnStartup = true;
}

public sealed class UIScreenRegistration
{
    public UIScreenId ScreenId { get; }
    public ViewBase View { get; }
    public IPresenter Presenter { get; }
    public bool ShowOnStartup { get; }

    public UIScreenRegistration(UIScreenId screenId, ViewBase view, IPresenter presenter, bool showOnStartup)
    {
        ScreenId = screenId;
        View = view;
        Presenter = presenter;
        ShowOnStartup = showOnStartup;
    }
}

// Builds concrete presenter instances from serialized bindings and returns runtime registrations.
public static class UIFactory
{
    private static readonly Dictionary<UIPresenterType, Func<UIPresenterBinding, IPresenter>> PresenterFactories =
        new()
        {
            { UIPresenterType.MainMenu, CreateMainMenuPresenter },
            { UIPresenterType.InGame, CreateInGamePresenter },
            { UIPresenterType.ChooseName, CreateChooseNamePresenter}
        };


    public static List<UIScreenRegistration> CreateRegistrations(IEnumerable<UIPresenterBinding> bindings)
    {
        var registrations = new List<UIScreenRegistration>();

        foreach (var binding in bindings)
        {
            if (!binding.View)
            {
                Debug.LogError($"[UIFactory] Missing view for screen {binding.ScreenId}.");
                continue;
            }

            if (!PresenterFactories.TryGetValue(binding.PresenterType, out var presenterFactory))
            {
                Debug.LogError($"[UIFactory] No presenter factory for type {binding.PresenterType}.");
                continue;
            }

            var presenter = presenterFactory(binding);
            if (presenter == null) continue;

            registrations.Add(new UIScreenRegistration(binding.ScreenId, binding.View, presenter, binding.ShowOnStartup));
        }

        return registrations;
    }

    private static IPresenter CreateMainMenuPresenter(UIPresenterBinding binding)
    {
        var mainMenuView = binding.View as MainMenuUIView;
        if (!mainMenuView)
        {
            Debug.LogError("[UIFactory] MainMenu presenter requires MainMenuUIView.");
            return null;
        }

        return new MainMenuUIPresenter(
            new MainMenuUIModel(),
            mainMenuView
        );
    }

    private static IPresenter CreateInGamePresenter(UIPresenterBinding binding)
    {
        var inGameView = binding.View as InGameUIView;
        if (!inGameView)
        {
            Debug.LogError("[UIFactory] InGame presenter requires InGameUIView.");
            return null;
        }

        return new InGameUIPresenter(
            new InGameUIModel(),
            inGameView
        );
    }

    private static IPresenter CreateChooseNamePresenter(UIPresenterBinding binding)
    {
        var chooseNameView = binding.View as ChooseNameUIView;
        if (!chooseNameView)
        {
            Debug.LogError("[UIFactory] InGame presenter requires InGameUIView.");
            return null;
        }

        return new ChooseNameUIPresenter(
            new ChooseNameUIModel(),
            chooseNameView
        );
    }
}
