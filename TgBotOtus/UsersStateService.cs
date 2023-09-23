public enum UserState
{
    NoState, fridgeEdit
}

public class UsersStateService
{
    // Dictionary не потокобезопасный. Но в нашем случае это не проблема.
    // ConcurrentDictionary - потокобезопасный аналог Dictionary.
    // В идеале - персистентное хранилище, например, база данных.
    private readonly Dictionary<long, UserState> _usersState = new();

    public void SetState(long userId, UserState state)
    {
        _usersState[userId] = state;
    }

    public UserState GetState(long userId)
    {
        if (_usersState.TryGetValue(userId, out var state))
        {
            return state;
        }
        else
        {
            return UserState.NoState;
        }
    }
}