using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;

namespace Dianty.Utils;

public class SvgPathConverter
{
    private SvgPathConverter(ReadOnlyMemory<char> svgPath)
    {
        _svgPath = svgPath;
    }

    private readonly ReadOnlyMemory<char> _svgPath;
    private readonly PathFigureCollection _figures = [];
    private FillRule _fillRule;
    private Command _lastCommand;
    private Point _lastPoint;
    private int _index;

    public static PathGeometry? Parse(ReadOnlyMemory<char> svgPath)
    {
        if (svgPath.Length == 0)
            return null;
        var converter = new SvgPathConverter(svgPath);
        converter.Run();
        var result = new PathGeometry
        {
            Figures = converter._figures,
            FillRule = converter._fillRule
        };
        return result;
    }

    private void Run()
    {
        var svgPath = _svgPath.Span;
        for (var i = 0; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            switch (c)
            {
                default:
                    Throw();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    _index++;
                    continue;
                case 'F':
                    _index++;
                    ProcessFillRule();
                    break;
                case 'M':
                    _index++;
                    Process_M();
                    break;
                case 'm':
                    _index++;
                    Process_m();
                    break;
            }
            break;
        }
        while (_index < svgPath.Length)
        {
            char c = svgPath[_index];
            switch (c)
            {
                default:
                    Throw();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    _index++;
                    break;
                case '-':
                case '.':
                case >= '0' and <= '9':
                    RepeatLastCommand();
                    break;
                case 'M':
                    _index++;
                    Process_M();
                    break;
                case 'm':
                    _index++;
                    Process_m();
                    break;
                case 'L':
                    _index++;
                    Process_L();
                    break;
                case 'l':
                    _index++;
                    Process_l();
                    break;
                case 'V':
                    _index++;
                    Process_V();
                    break;
                case 'v':
                    _index++;
                    Process_v();
                    break;
                case 'H':
                    _index++;
                    Process_H();
                    break;
                case 'h':
                    _index++;
                    Process_h();
                    break;
                case 'C':
                case 'c':
                case 'S':
                case 's':
                case 'Q':
                case 'q':
                case 'T':
                case 't':
                case 'A':
                case 'a':
                    throw new NotImplementedException("当前仅支持线段");
                case 'Z':
                case 'z':
                    _index++;
                    _lastCommand = Command.Z;
                    var pathFigure = _figures[^1];
                    pathFigure.IsClosed = true;
                    _lastPoint = pathFigure.StartPoint;
                    break;
            }
        }
    }

    private void RepeatLastCommand()
    {
        switch (_lastCommand)
        {
            default:
            case Command.None:
            case Command.F:
            case Command.Z:
                Throw();
                break;
            case Command.M:
                Process_M();
                break;
            case Command.m:
                Process_m();
                break;
            case Command.L:
                Process_L();
                break;
            case Command.l:
                Process_l();
                break;
            case Command.V:
                Process_V();
                break;
            case Command.v:
                Process_v();
                break;
            case Command.H:
                Process_H();
                break;
            case Command.h:
                Process_h();
                break;
            case Command.C:
            case Command.c:
            case Command.S:
            case Command.s:
            case Command.Q:
            case Command.q:
            case Command.T:
            case Command.t:
            case Command.A:
            case Command.a:
                throw new NotImplementedException("当前仅支持线段");
        }
    }

    private void ProcessFillRule()
    {
        if (_lastCommand != Command.None)
            Throw();
        _lastCommand = Command.F;
        var svgPath = _svgPath.Span;
        for (var i = _index; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            switch (c)
            {
                default:
                    Throw();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    continue;
                case '0':
                    _fillRule = FillRule.EvenOdd;
                    break;
                case '1':
                    _fillRule = FillRule.Nonzero;
                    break;
            }
            _index = i + 1;
            break;
        }
        for (var i = _index; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            switch (c)
            {
                default:
                    Throw();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    continue;
                case 'M':
                    _index = i + 1;
                    Process_M();
                    break;
                case 'm':
                    _index = i + 1;
                    Process_m();
                    break;
            }
            break;
        }
    }

    private void Process_M()
    {
        if (_figures.Count > 0 && _lastCommand != Command.Z)
            _figures[^1].IsClosed = false;
        _lastCommand = Command.M;
        var x = GetNumber();
        var y = GetNumber();
        var startPoint = new Point(x, y);
        _figures.Add(new PathFigure { StartPoint = startPoint });
        _lastPoint = startPoint;
    }

    private void Process_m()
    {
        if (_figures.Count > 0 && _lastCommand != Command.Z)
            _figures[^1].IsClosed = false;
        _lastCommand = Command.m;
        var x = GetNumber();
        var y = GetNumber();
        var startPoint = new Point(_lastPoint._x + x, _lastPoint._y + y);
        _figures.Add(new PathFigure { StartPoint = startPoint });
        _lastPoint = startPoint;
    }

    private void Process_L()
    {
        _lastCommand = Command.L;
        var x = GetNumber();
        var y = GetNumber();
        var newPoint = new Point(x, y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private void Process_l()
    {
        _lastCommand = Command.l;
        var x = GetNumber();
        var y = GetNumber();
        var newPoint = new Point(_lastPoint._x + x, _lastPoint._y + y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private void Process_V()
    {
        _lastCommand = Command.V;
        var y = GetNumber();
        var newPoint = new Point(_lastPoint._x, y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private void Process_v()
    {
        _lastCommand = Command.v;
        var y = GetNumber();
        var newPoint = new Point(_lastPoint._x, _lastPoint._y + y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private void Process_H()
    {
        _lastCommand = Command.H;
        var x = GetNumber();
        var newPoint = new Point(x, _lastPoint._y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private void Process_h()
    {
        _lastCommand = Command.h;
        var x = GetNumber();
        var newPoint = new Point(_lastPoint._x + x, _lastPoint._y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastPoint = newPoint;
    }

    private float GetNumber()
    {
        var svgPath = _svgPath.Span;
        for (var i = _index; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            switch (c)
            {
                default:
                    Throw();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    continue;
                case '-':
                case '.':
                case >= '0' and <= '9':
                    var start = i;
                    for (i++; i < svgPath.Length; i++)
                    {
                        c = svgPath[i];
                        if ((c < '0' || c > '9') && c != '.')
                            break;
                    }
                    _index = i;
                    return float.Parse(svgPath[start..i]);
            }
        }
        Throw();
        return 0;
    }

    [DoesNotReturn]
    private void Throw()
    {
        throw new ArgumentException($"在[{_index}]出现了意外的字符");
    }

    private enum Command
    {
        None, F, M, m, L, l, V, v, H, h, C, c, S, s, Q, q, T, t, A, a, Z
    }
}
