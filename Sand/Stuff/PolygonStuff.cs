//using FlatRedBall.Math.Geometry;
//using Sand.Game;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Sand.Constants;
//using Color = Microsoft.Xna.Framework.Color;

//namespace Sand.Stuff;

//public class PolygonStuff : AbstractStuff
//{
//	private Polygon _polygon;

//	public PolygonStuff(StuffDescriptor descriptor) : base(descriptor)
//	{
//		try
//		{

//			_polygon = PolygonService.Instance.GetColouredPolygon(new Color(descriptor.ColorRgba[0], descriptor.ColorRgba[1], descriptor.ColorRgba[2], descriptor.ColorRgba[3]));
//		}
//		catch (Exception ex)
//		{
//			_polygon = PolygonService.Instance.GetColouredPolygon(new Color(255, 0, 0, 255));
//			Logger.Instance.LogError(ex, $"PolygonStuff ctor ERROR - made one anyway and made it RED");
//		}
//		finally 
//		{ 
//			_polygon.XAcceleration = 0;
//			_polygon.YAcceleration = 0;
//		}
//	}

//	~PolygonStuff()
//	{
//		this._polygon.Visible = false;
//		ShapeManager.Remove(this._polygon);
//	}

//	public override AbstractStuff SetPosition(int x, int y)
//	{
//		//_polygon.X = x;// * STUFF_SCALE /*+ _polygon.X*/ / 2;
//		//_polygon.Y = y;// * STUFF_SCALE /*+ _polygon.X*/ / 2;
//		_polygon.X = x * STUFF_SCALE;// + _sprite.Width / 2;
//		_polygon.Y = y * STUFF_SCALE;// + _sprite.Height / 2;
//		_polygon.Points = [
//			new (-STUFF_SCALE, -STUFF_SCALE),
//			new (-STUFF_SCALE, STUFF_SCALE),
//			new (STUFF_SCALE, STUFF_SCALE),
//			new (STUFF_SCALE, -STUFF_SCALE)
//		];
//		//_polygon.BoundingRectangle = new FloatRectangle(new Point(x, y), STUFF_SCALE, STUFF_SCALE);
//		return this;
//	}
//}
