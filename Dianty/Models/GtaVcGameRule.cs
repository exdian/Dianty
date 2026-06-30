using GameMonitor;
using System;
using static Dianty.Models.GameManager;
using static GameMonitor.GtaVcMonitor;

namespace Dianty.Models;

public class GtaVcGameRule : GameRule
{
    public GtaVcGameRule(IMemoryService memoryService)
    {
        _monitor = new GtaVcMonitor(memoryService);
        _monitor.PlayerBusted += Monitor_PlayerBusted;
        _monitor.PlayerWasted += Monitor_PlayerWasted;
        _monitor.PlayerWantedLevelChanged += Monitor_PlayerWantedLevelChanged;
        _monitor.PlayerFellOffBike += Monitor_PlayerFellOffBike;
    }

    ~GtaVcGameRule()
    {
        _monitor.Stop();
    }

    private readonly GtaVcMonitor _monitor;
    private float _playerTookDamageTotal;
    private int _damageRuleOutputStrength;

    public override bool IsEnable
    {
        get;
        set
        {
            if (value)
                _monitor.Start();
            else
                _monitor.Stop();
            field = value;
        }
    }

    public bool DamageRuleEnable
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                if (value)
                    _monitor.PlayerTookDamage += Monitor_PlayerTookDamage;
                else
                    _monitor.PlayerTookDamage -= Monitor_PlayerTookDamage;
                _playerTookDamageTotal = 0;
                _damageRuleOutputStrength = 0;
            }
        }
    }

    public int DamageRuleThreshold
    {
        get;
        set
        {
            field = value;
            ComputePlayerTookDamageOutputStrength();
        }
    }

    public int DamageRuleStrength { get; set; }

    private void Monitor_PlayerTookDamage(object? sender, PlayerTookDamageEventArgs e)
    {
        var damage = e.Damage;
        if (DamageRuleThreshold > 0 && damage > 0)
        {
            if (_playerTookDamageTotal < 0)
            {
                _playerTookDamageTotal = 0;
            }
        }
        else if (DamageRuleThreshold < 0 && damage < 0)
        {
            if (_playerTookDamageTotal > 0)
            {
                _playerTookDamageTotal = 0;
            }
        }
        else
        {
            return;
        }
        _playerTookDamageTotal = _playerTookDamageTotal + damage;
        ComputePlayerTookDamageOutputStrength();
    }

    private void Monitor_PlayerBusted(object? sender, PlayerBustedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerWasted(object? sender, PlayerWastedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerWantedLevelChanged(object? sender, PlayerWantedLevelChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerFellOffBike(object? sender, PlayerFellOffBikeEventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override int GetMaxStrength()
    {
        int result = 0;
        if (_damageRuleOutputStrength > result)
            result = _damageRuleOutputStrength;
        return result;
    }

    protected override int SumStrength()
    {
        int result = 0;
        if (_damageRuleOutputStrength > 0)
            result = result + _damageRuleOutputStrength;
        return result;
    }

    private void ComputePlayerTookDamageOutputStrength()
    {
        if (!IsEnable || !DamageRuleEnable)
        {
            _damageRuleOutputStrength = 0;
        }
        else if ((DamageRuleThreshold > 0 && _playerTookDamageTotal > DamageRuleThreshold)
            || (DamageRuleThreshold < 0 && _playerTookDamageTotal < DamageRuleThreshold))
        {
            var multiple = _playerTookDamageTotal / DamageRuleThreshold;
            _playerTookDamageTotal %= DamageRuleThreshold;
            _damageRuleOutputStrength = _damageRuleOutputStrength + (int)(multiple * DamageRuleStrength);
        }
        else
        {
            return;
        }
        ComputeOutputStrength();
    }
}
