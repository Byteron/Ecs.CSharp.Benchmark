﻿using BenchmarkDotNet.Attributes;
using Ecs.CSharp.Benchmark.Context;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace Ecs.CSharp.Benchmark
{
    public partial class SystemWithTwoComponents
    {
        private class MonoGameExtendedContext : MonoGameExtendedBaseContext
        {
            public class UpdateSystem : EntityUpdateSystem
            {
                private ComponentMapper<Component1> _c1;
                private ComponentMapper<Component2> _c2;

                public UpdateSystem()
                    : base(Aspect.All(typeof(Component1), typeof(Component2)))
                { }

                public override void Initialize(IComponentMapperService mapperService)
                {
                    _c1 = mapperService.GetMapper<Component1>();
                    _c2 = mapperService.GetMapper<Component2>();
                }

                public override void Update(GameTime gameTime)
                {
                    foreach (int entityId in ActiveEntities)
                    {
                        _c1.Get(entityId).Value += _c2.Get(entityId).Value;
                    }
                }
            }

            private readonly UpdateSystem _system;

            public new World World { get; }

            public GameTime Time { get; }

            public MonoGameExtendedContext(int entityCount, int entityPadding)
            {
                _system = new UpdateSystem();
                World = new WorldBuilder().AddSystem(_system).Build();
                Time = new GameTime();

                for (int i = 0; i < entityCount; ++i)
                {
                    for (int j = 0; j < entityPadding; ++j)
                    {
                        Entity padding = World.CreateEntity();
                        switch (j % 2)
                        {
                            case 0:
                                padding.Attach(new Component1());
                                break;

                            case 1:
                                padding.Attach(new Component2());
                                break;
                        }
                    }

                    Entity entity = World.CreateEntity();
                    entity.Attach(new Component1());
                    entity.Attach(new Component2 { Value = 1 });
                }
            }

            public override void Dispose()
            {
                World.Dispose();
                _system.Dispose();

                base.Dispose();
            }
        }

        [Context]
        private readonly MonoGameExtendedContext _monoGameExtended;

        [BenchmarkCategory(Categories.MonoGameExtended)]
        [Benchmark]
        public void MonoGameExtended() => _monoGameExtended.World.Update(_monoGameExtended.Time);
    }
}
