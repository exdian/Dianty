// SPDX-License-Identifier: GPL-3.0-only OR MIT

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
    private Point _lastControlPoint;
    private Point _lastEndPoint;
    private int _index;

    // 测试用例 M5 .0 -.0e-1 5 .1e02 5C8.3333 3.3333 6.6667 1.6667 .5e1 -0
    // 一个三角形 5,0  0,5  10,5
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
                    _index++;
                    Process_C();
                    break;
                case 'c':
                    _index++;
                    Process_c();
                    break;
                case 'S':
                    _index++;
                    Process_S();
                    break;
                case 's':
                    _index++;
                    Process_s();
                    break;
                case 'Q':
                    _index++;
                    Process_Q();
                    break;
                case 'q':
                    _index++;
                    Process_q();
                    break;
                case 'T':
                    _index++;
                    Process_T();
                    break;
                case 't':
                    _index++;
                    Process_t();
                    break;
                case 'A':
                    _index++;
                    Process_A();
                    break;
                case 'a':
                    _index++;
                    Process_a();
                    break;
                case 'Z':
                case 'z':
                    _index++;
                    _lastCommand = Command.Z;
                    var pathFigure = _figures[^1];
                    pathFigure.IsClosed = true;
                    _lastEndPoint = pathFigure.StartPoint;
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
            case Command.L:
                Process_L();
                break;
            case Command.m:
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
                Process_C();
                break;
            case Command.c:
                Process_c();
                break;
            case Command.S:
                Process_S();
                break;
            case Command.s:
                Process_s();
                break;
            case Command.Q:
                Process_Q();
                break;
            case Command.q:
                Process_q();
                break;
            case Command.T:
                Process_T();
                break;
            case Command.t:
                Process_t();
                break;
            case Command.A:
                Process_A();
                break;
            case Command.a:
                Process_a();
                break;
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
        float x = GetNumber();
        float y = GetNumber();
        var startPoint = new Point(x, y);
        _figures.Add(new PathFigure { StartPoint = startPoint });
        _lastEndPoint = startPoint;
    }

    private void Process_m()
    {
        if (_figures.Count > 0 && _lastCommand != Command.Z)
            _figures[^1].IsClosed = false;
        _lastCommand = Command.m;
        float x = GetNumber();
        float y = GetNumber();
        var startPoint = new Point(_lastEndPoint._x + x, _lastEndPoint._y + y);
        _figures.Add(new PathFigure { StartPoint = startPoint });
        _lastEndPoint = startPoint;
    }

    private void Process_L()
    {
        _lastCommand = Command.L;
        float x = GetNumber();
        float y = GetNumber();
        var newPoint = new Point(x, y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_l()
    {
        _lastCommand = Command.l;
        float x = GetNumber();
        float y = GetNumber();
        var newPoint = new Point(_lastEndPoint._x + x, _lastEndPoint._y + y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_V()
    {
        _lastCommand = Command.V;
        float y = GetNumber();
        var newPoint = new Point(_lastEndPoint._x, y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_v()
    {
        _lastCommand = Command.v;
        float y = GetNumber();
        var newPoint = new Point(_lastEndPoint._x, _lastEndPoint._y + y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_H()
    {
        _lastCommand = Command.H;
        float x = GetNumber();
        var newPoint = new Point(x, _lastEndPoint._y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_h()
    {
        _lastCommand = Command.h;
        float x = GetNumber();
        var newPoint = new Point(_lastEndPoint._x + x, _lastEndPoint._y);
        _figures[^1].Segments.Add(new LineSegment { Point = newPoint });
        _lastEndPoint = newPoint;
    }

    private void Process_C()
    {
        _lastCommand = Command.C;
        float x1 = GetNumber();
        float y1 = GetNumber();
        float x2 = GetNumber();
        float y2 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint2 = new Point(x2, y2);
        var newPoint = new Point(x, y);
        var bezierSegment = new BezierSegment
        {
            Point1 = new Point(x1, y1),
            Point2 = controlPoint2,
            Point3 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint2;
        _lastEndPoint = newPoint;
    }

    private void Process_c()
    {
        _lastCommand = Command.c;
        float x1 = GetNumber();
        float y1 = GetNumber();
        float x2 = GetNumber();
        float y2 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var lastEndPoint = _lastEndPoint;
        var controlPoint2 = new Point(lastEndPoint._x + x2, lastEndPoint._y + y2);
        var newPoint = new Point(lastEndPoint._x + x, lastEndPoint._y + y);
        var bezierSegment = new BezierSegment
        {
            Point1 = new Point(lastEndPoint._x + x1, lastEndPoint._y + y1),
            Point2 = controlPoint2,
            Point3 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint2;
        _lastEndPoint = newPoint;
    }

    private void Process_S()
    {
        Point lastControlPoint;
        if (_lastCommand < Command.C || _lastCommand > Command.s)
            lastControlPoint = _lastEndPoint;
        else
            lastControlPoint = _lastControlPoint;
        _lastCommand = Command.S;
        var lastEndPoint = _lastEndPoint;
        float x1 = lastEndPoint._x - lastControlPoint._x + lastEndPoint._x;
        float y1 = lastEndPoint._y - lastControlPoint._y + lastEndPoint._y;
        float x2 = GetNumber();
        float y2 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint2 = new Point(x2, y2);
        var newPoint = new Point(x, y);
        var bezierSegment = new BezierSegment
        {
            Point1 = new Point(x1, y1),
            Point2 = controlPoint2,
            Point3 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint2;
        _lastEndPoint = newPoint;
    }

    private void Process_s()
    {
        Point lastControlPoint;
        if (_lastCommand < Command.C || _lastCommand > Command.s)
            lastControlPoint = _lastEndPoint;
        else
            lastControlPoint = _lastControlPoint;
        _lastCommand = Command.s;
        var lastEndPoint = _lastEndPoint;
        float x1 = lastEndPoint._x - lastControlPoint._x + lastEndPoint._x;
        float y1 = lastEndPoint._y - lastControlPoint._y + lastEndPoint._y;
        float x2 = GetNumber();
        float y2 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint2 = new Point(lastEndPoint._x + x2, lastEndPoint._y + y2);
        var newPoint = new Point(lastEndPoint._x + x, lastEndPoint._y + y);
        var bezierSegment = new BezierSegment
        {
            Point1 = new Point(lastEndPoint._x + x1, lastEndPoint._y + y1),
            Point2 = controlPoint2,
            Point3 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint2;
        _lastEndPoint = newPoint;
    }

    private void Process_Q()
    {
        _lastCommand = Command.Q;
        float x1 = GetNumber();
        float y1 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint1 = new Point(x1, y1);
        var newPoint = new Point(x, y);
        var bezierSegment = new QuadraticBezierSegment
        {
            Point1 = controlPoint1,
            Point2 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint1;
        _lastEndPoint = newPoint;
    }

    private void Process_q()
    {
        _lastCommand = Command.q;
        float x1 = GetNumber();
        float y1 = GetNumber();
        float x = GetNumber();
        float y = GetNumber();
        var lastEndPoint = _lastEndPoint;
        var controlPoint1 = new Point(lastEndPoint._x + x1, lastEndPoint._y + y1);
        var newPoint = new Point(lastEndPoint._x + x, lastEndPoint._y + y);
        var bezierSegment = new QuadraticBezierSegment
        {
            Point1 = controlPoint1,
            Point2 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint1;
        _lastEndPoint = newPoint;
    }

    private void Process_T()
    {
        Point lastControlPoint;
        if (_lastCommand < Command.Q || _lastCommand > Command.t)
            lastControlPoint = _lastEndPoint;
        else
            lastControlPoint = _lastControlPoint;
        _lastCommand = Command.T;
        var lastEndPoint = _lastEndPoint;
        float x1 = lastEndPoint._x - lastControlPoint._x + lastEndPoint._x;
        float y1 = lastEndPoint._y - lastControlPoint._y + lastEndPoint._y;
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint1 = new Point(x1, y1);
        var newPoint = new Point(x, y);
        var bezierSegment = new QuadraticBezierSegment
        {
            Point1 = controlPoint1,
            Point2 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint1;
        _lastEndPoint = newPoint;
    }

    private void Process_t()
    {
        Point lastControlPoint;
        if (_lastCommand < Command.Q || _lastCommand > Command.t)
            lastControlPoint = _lastEndPoint;
        else
            lastControlPoint = _lastControlPoint;
        _lastCommand = Command.t;
        var lastEndPoint = _lastEndPoint;
        float x1 = lastEndPoint._x - lastControlPoint._x + lastEndPoint._x;
        float y1 = lastEndPoint._y - lastControlPoint._y + lastEndPoint._y;
        float x = GetNumber();
        float y = GetNumber();
        var controlPoint1 = new Point(x1, y1);
        var newPoint = new Point(lastEndPoint._x + x, lastEndPoint._y + y);
        var bezierSegment = new QuadraticBezierSegment
        {
            Point1 = controlPoint1,
            Point2 = newPoint,
        };
        _figures[^1].Segments.Add(bezierSegment);
        _lastControlPoint = controlPoint1;
        _lastEndPoint = newPoint;
    }

    private void Process_A()
    {
        _lastCommand = Command.A;
        float width = GetNumber();
        float height = GetNumber();
        float rotation = GetNumber();
        bool isLargeArc = GetFlag();
        bool isClockwise = GetFlag();
        float x = GetNumber();
        float y = GetNumber();
        var newPoint = new Point(x, y);
        var arcSegment = new ArcSegment
        {
            Size = new Size(width, height),
            RotationAngle = rotation,
            IsLargeArc = isLargeArc,
            SweepDirection = isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
            Point = newPoint,
        };
        _figures[^1].Segments.Add(arcSegment);
        _lastEndPoint = newPoint;
    }

    private void Process_a()
    {
        _lastCommand = Command.a;
        float width = GetNumber();
        float height = GetNumber();
        float rotation = GetNumber();
        bool isLargeArc = GetFlag();
        bool isClockwise = GetFlag();
        float x = GetNumber();
        float y = GetNumber();
        var newPoint = new Point(_lastEndPoint._x + x, _lastEndPoint._y + y);
        var arcSegment = new ArcSegment
        {
            Size = new Size(width, height),
            RotationAngle = rotation,
            IsLargeArc = isLargeArc,
            SweepDirection = isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
            Point = newPoint,
        };
        _figures[^1].Segments.Add(arcSegment);
        _lastEndPoint = newPoint;
    }

    private float GetNumber()
    {
        var svgPath = _svgPath.Span;
        for (var i = _index; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            int start;
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
                case >= '0' and <= '9':
                    start = i;
                    for (i++; i < svgPath.Length; i++)
                    {
                        c = svgPath[i];
                        if (c >= '0' && c <= '9')
                            continue;
                        if (c == '.')
                        {
                            i++;
                            i = GetDecimalEndIndex(svgPath, i);
                        }
                        else if (c == 'E' || c == 'e')
                        {
                            i++;
                            i = GetExponentEndIndex(svgPath, i);
                        }
                        break;
                    }
                    _index = i;
                    return float.Parse(svgPath[start..i]);
                case '.':
                    start = i;
                    i++;
                    i = GetDecimalEndIndex(svgPath, i);
                    _index = i;
                    return float.Parse(svgPath[start..i]);
            }
        }
        Throw();
        return 0;
    }

    private bool GetFlag()
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
                case '0':
                    _index = i + 1;
                    return false;
                case '1':
                    _index = i + 1;
                    return true;
            }
        }
        Throw();
        return default;
    }

    [DoesNotReturn]
    private void Throw()
    {
        throw new ArgumentException($"在[{_index}]出现了意外的字符");
    }

    private static int GetDecimalEndIndex(ReadOnlySpan<char> svgPath, int index)
    {
        int i;
        for (i = index; i < svgPath.Length; i++)
        {
            char c = svgPath[i];
            if (c >= '0' && c <= '9')
                continue;
            if (c == 'E' || c == 'e')
            {
                i++;
                i = GetExponentEndIndex(svgPath, i);
            }
            break;
        }
        return i;
    }

    private static int GetExponentEndIndex(ReadOnlySpan<char> svgPath, int index)
    {
        int i = index;
        char c = svgPath[i];
        if (c == '-' || (c >= '0' && c <= '9'))
        {
            for (i++; i < svgPath.Length; i++)
            {
                c = svgPath[i];
                if (c < '0' || c > '9')
                    break;
            }
        }
        return i;
    }

    private enum Command
    {
        None, F, M, m, L, l, V, v, H, h, C, c, S, s, Q, q, T, t, A, a, Z
    }
}
