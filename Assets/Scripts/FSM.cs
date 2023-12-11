using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FSM
{
    //El estado que voy a crear con su nombre (String)) y lo q va a hacer (IState)
    Dictionary<string, IState> _states = new Dictionary<string, IState>();

    IState _actualState;


    /// <summary>
    /// Crear estado y guardarlo en el diccionario
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public void CreateState(string name, IState state)
    {
        if (!_states.ContainsKey(name))
            _states.Add(name, state);
    }

    /// <summary>
    /// Se va a ejecutar todo el tiempo en el OnUpdate();
    /// </summary>
    public void Execute()
    {
        _actualState.OnUpdate();
    }

    /// <summary>
    /// El estado al que quiero cambiar
    /// </summary>
    /// <param name="name"></param>
    public void ChangeState(string name)
    {
        if (_states.ContainsKey(name))
        {
            //if (_actualState != _states[name])
            //{
            //    if (_actualState != null)
            //        _actualState.OnExit();

            //    _actualState = _states[name];
            //    _actualState.OnEnter();
            //}

            if (_actualState != null)
                _actualState.OnExit();

            _actualState = _states[name];
            _actualState.OnEnter();
        }
    }
}
