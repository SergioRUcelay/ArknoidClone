using System;

namespace Arkanoid_02
{
    public static class Walls
    {
        public static Action OnHit { get; set; }

        //public Segment[] WallGetSegments()
        //{
        //    return new Segment[]
        //    {
        //            new() {end = brick_pos+new Vector2 (BrickWidth,0),           ini = brick_pos+new Vector2(BrickWidth,BrickHeight), owner = this, act_Segment = true},
        //            new() {end = brick_pos+new Vector2(BrickWidth,BrickHeight),  ini = brick_pos+new Vector2(0,BrickHeight),          owner = this, act_Segment = true},
        //            new() {end = brick_pos+new Vector2(0,BrickHeight),           ini = brick_pos,                                     owner = this, act_Segment = true},
        //            new() {end = brick_pos,                                      ini = brick_pos+new Vector2 (BrickWidth,0),          owner = this, act_Segment = true},
        //    };
        //}
    }
    

}
