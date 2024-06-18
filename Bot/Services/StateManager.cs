using Bot.Commands.Abstractions;

namespace Bot.Services;

public class StateManager(IServiceProvider provider) {
    // userId / command
    private Dictionary<long, string> _states = new();

    public IStateHandler? TryPop(long userId) {
        if (_states.Remove(userId, out var state)) {
            return provider.GetRequiredKeyedService<IStateHandler>(state);
        }

        return null;
    }

    public bool Set(long userId, string state, bool replace = true) {
        var result = !_states.TryAdd(userId, state);
        
        if (result && replace) {
            _states[userId] = state;
            
            return true;
        }

        return !result;
    }
}