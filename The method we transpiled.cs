
// This is the method that we did the Transpile on

public void UpdatePlayerBody(PlayerBodyV2 playerBody)
{
    if (!playerBody || !playerBodyVisualElementMap.ContainsKey(playerBody))
    {
        return;
    }
    VisualElement e = playerBodyVisualElementMap[playerBody];
    Player player = playerBody.Player;
    if ((bool)player)
    {
        VisualElement visualElement = e.Query<VisualElement>("Body");
        VisualElement visualElement2 = e.Query<VisualElement>("Local");
        Label label = e.Query<Label>("Number");
        switch (player.Team.Value)
        {
            case PlayerTeam.Blue:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(teamBlueColor);
                break;
            case PlayerTeam.Red:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(teamRedColor);
                break;
            default:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
                break;
        }
        label.text = player.Number.Value.ToString();
        label.style.visibility = (player.IsLocalPlayer ? Visibility.Hidden : Visibility.Visible);
        visualElement2.style.visibility = ((!player.IsLocalPlayer) ? Visibility.Hidden : Visibility.Visible);
    }
}


// And this is what it should be Transpiled to

public void UpdatePlayerBody(PlayerBodyV2 playerBody)
{
    if (!playerBody || !playerBodyVisualElementMap.ContainsKey(playerBody))
    {
        return;
    }
    VisualElement e = playerBodyVisualElementMap[playerBody];
    Player player = playerBody.Player;
    if ((bool)player)
    {
        VisualElement visualElement = e.Query<VisualElement>("Body");
        VisualElement visualElement2 = e.Query<VisualElement>("Local");
        Label label = e.Query<Label>("Number");
        switch (player.Team.Value)
        {
            case PlayerTeam.Blue:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(teamBlueColor);
                label.style.color = Color.black; // Line added
                break;
            case PlayerTeam.Red:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(teamRedColor);
                label.style.color = Color.black; // Line added
                break;
            default:
                visualElement.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
                break;
        }
        label.text = player.Number.Value.ToString();
        label.style.visibility = (player.IsLocalPlayer ? Visibility.Hidden : Visibility.Visible);
        visualElement2.style.visibility = ((!player.IsLocalPlayer) ? Visibility.Hidden : Visibility.Visible);
    }
}

// All of that Transpile logic added 2 lines of code, the joys of IL code