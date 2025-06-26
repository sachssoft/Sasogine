using sachssoft.Sasogine.Surface;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sachssoft.Sasogine.Interactions;

public class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void RegisterCommand(string key, ICommand command)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        _commands[key] = command ?? throw new ArgumentNullException(nameof(command));
    }

    public void UnregisterCommand(string key)
    {
        _commands.Remove(key);
    }

    public bool TryGetCommand(string key, out ICommand? command)
    {
        return _commands.TryGetValue(key, out command);
    }

    public bool TryGetKey(ICommand? command, [MaybeNullWhen(false)] out string key)
    {
        key = null;

        if (command == null)
        {
            return false;
        }

        foreach (var pair in _commands)
        {
            if (ReferenceEquals(pair.Value, command))
            {
                key = pair.Key;
                return true;
            }
        }

        return false;
    }

    public void Execute(ViewBase view, string key, params object[] args)
    {
        if (_commands.TryGetValue(key, out var command) && command.CanExecute(view, args))
        {
            command.Execute(view, args);
        }
    }

    public void RaiseCanExecuteChanged(string key)
    {
        if (_commands.TryGetValue(key, out var command))
        {
            if (command is ActionCommand cmd)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }
    }

    public void RaiseAllCanExecuteChanged()
    {
        foreach (var command in _commands.Values)
        {
            if (command is ActionCommand cmd)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }
    }

    public IEnumerable<string> Keys => _commands.Keys;

    public void Clear()
    {
        _commands.Clear();
    }
}
