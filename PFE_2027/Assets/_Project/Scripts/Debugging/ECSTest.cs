using System;
using System.Collections.Generic;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Debugging._Project.Scripts.Debugging
{
    public class ECSTest : MonoPhaseListener<CrossRoadGameMode>
    {
        private World world;
        private List<Entity> current = new List<Entity>();
        public int[] entities;

        protected override void OnPhaseBegin(CrossRoadGameMode phase)
        {
            base.OnPhaseBegin(phase);

            world = phase.world;
        }

        protected override void OnPhaseEnd(CrossRoadGameMode phase)
        {
            base.OnPhaseEnd(phase);
            world = null;
        }

        [Button]
        public void AddEntity()
        {
            current.Add(world.CreateEntity());
            entities = world.Entities.ToArray();
        }

        [Button]
        public void RemoveEntityFirst()
        {
            world.DestroyEntity(current[0]);
            current.RemoveAt(0);
            entities = world.Entities.ToArray();
        }
        
        [Button]
        public void TestStaleHandle()
        {
            Entity stale = world.CreateEntity();
            world.DestroyEntity(stale);              // the id goes back to freeIds, its generation is bumped
            Entity fresh = world.CreateEntity();     // pops the same id

            Debug.Log($"Same id: {stale.id == fresh.id}");          // expected True
            Debug.Log($"Stale: {stale}, fresh: {fresh}");           // e.g. Entity(3, 0) / Entity(3, 1)
            Debug.Log($"Stale alive: {world.IsAlive(stale)}");      // expected False
            Debug.Log($"Fresh alive: {world.IsAlive(fresh)}");      // expected True
            Debug.Log($"Equal: {stale == fresh}");                  // expected False

            world.DestroyEntity(stale);                             // must do NOTHING (stale handle)
            Debug.Log($"Fresh still alive: {world.IsAlive(fresh)}"); // expected True

            world.DestroyEntity(fresh);
        }

        // ---------------------------------------------------------------- components

        // Components used only by the tests below
        private struct TestHealth : IEntityComponent
        {
            public int value;
        }

        private struct TestTag : IEntityComponent { }

        private struct TestMove : IEntityComponent { }

        private int passed;
        private int failed;

        // Runs every component test on the phase world: each test creates its own entities and destroys them
        [Button]
        public void TestComponents()
        {
            if (world == null)
            {
                Debug.LogWarning("[ECSTest] No world: start the game mode first.");
                return;
            }

            passed = 0;
            failed = 0;

            Run(nameof(TestAddAndRead), TestAddAndRead);
            Run(nameof(TestOverwrite), TestOverwrite);
            Run(nameof(TestModifyByRef), TestModifyByRef);
            Run(nameof(TestTryGetMissing), TestTryGetMissing);
            Run(nameof(TestRemove), TestRemove);
            Run(nameof(TestRemoveInTheMiddle), TestRemoveInTheMiddle);
            Run(nameof(TestDestroyCleansComponents), TestDestroyCleansComponents);
            Run(nameof(TestStaleHandleComponents), TestStaleHandleComponents);

            Debug.Log($"[ECSTest] Components: {passed} passed, {failed} failed");
        }

        // A test that throws counts as a failure, the next ones still run
        private void Run(string testName, Action test)
        {
            try
            {
                test();
            }
            catch (Exception exception)
            {
                failed++;
                Debug.LogError($"[ECSTest] CRASH {testName}: {exception.Message}");
            }
        }

        private void Check(bool condition, string label)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"[ECSTest] OK   {label}");
            }
            else
            {
                failed++;
                Debug.LogError($"[ECSTest] FAIL {label}");
            }
        }

        private void TestAddAndRead()
        {
            Entity entity = world.CreateEntity();

            Check(world.AddComponent(entity, new TestHealth { value = 10 }), "Add returns true for a new component");
            Check(world.HasComponent<TestHealth>(entity), "Has is true after Add");
            Check(world.TryGetComponent(entity, out TestHealth health) && health.value == 10, "TryGet reads the added value");
            Check(!world.HasComponent<TestTag>(entity), "Has is false for a component never added");

            world.DestroyEntity(entity);
        }

        private void TestOverwrite()
        {
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestHealth { value = 10 });

            Check(!world.AddComponent(entity, new TestHealth { value = 20 }), "Add returns false when overwriting");
            Check(world.TryGetComponent(entity, out TestHealth health) && health.value == 20, "Overwrite keeps the new value");

            world.DestroyEntity(entity);
        }

        private void TestModifyByRef()
        {
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestHealth { value = 20 });

            if (world.TryGetComponentRef(entity, out ComponentRef<TestHealth> healthRef))
                healthRef.GetValue().value -= 5;

            Check(world.TryGetComponent(entity, out TestHealth health) && health.value == 15, "A change through the ref is kept");

            world.DestroyEntity(entity);
        }

        private void TestTryGetMissing()
        {
            Entity entity = world.CreateEntity();

            // Expected: TryGetComponentRef logs an error here, but must not throw
            Check(!world.TryGetComponent(entity, out TestTag _), "TryGet is false without the component (no exception)");

            world.DestroyEntity(entity);
        }

        private void TestRemove()
        {
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestHealth { value = 10 });

            Check(world.RemoveComponent<TestHealth>(entity), "Remove returns true");
            Check(!world.HasComponent<TestHealth>(entity), "Has is false after Remove (mask cleared)");
            Check(!world.TryGetComponent(entity, out TestHealth _), "TryGet is false after Remove");
            Check(!world.RemoveComponent<TestHealth>(entity), "A second Remove returns false");

            world.DestroyEntity(entity);
        }

        // Removing the first one moves the last one into its slot: the others must keep their own values
        private void TestRemoveInTheMiddle()
        {
            Entity first = world.CreateEntity();
            Entity second = world.CreateEntity();
            Entity third = world.CreateEntity();
            world.AddComponent(first, new TestHealth { value = 1 });
            world.AddComponent(second, new TestHealth { value = 2 });
            world.AddComponent(third, new TestHealth { value = 3 });

            world.RemoveComponent<TestHealth>(first);

            Check(world.TryGetComponent(second, out TestHealth secondHealth) && secondHealth.value == 2, "Second keeps its value after the swap");
            Check(world.TryGetComponent(third, out TestHealth thirdHealth) && thirdHealth.value == 3, "Third (moved) keeps its value after the swap");

            world.DestroyEntity(first);
            world.DestroyEntity(second);
            world.DestroyEntity(third);
        }

        private void TestDestroyCleansComponents()
        {
            Entity destroyed = world.CreateEntity();
            world.AddComponent(destroyed, new TestHealth { value = 10 });
            world.AddComponent(destroyed, new TestTag());
            world.DestroyEntity(destroyed);

            // freeIds is a stack: the next entity gets the id just freed
            Entity reused = world.CreateEntity();

            Check(reused.id == destroyed.id, "The new entity reuses the destroyed id");
            Check(!world.HasComponent<TestHealth>(reused) && !world.HasComponent<TestTag>(reused), "The reused id has no leftover component in its mask");
            Check(world.TryGetStore(out ComponentStore<TestHealth> store) && !store.Has(reused.id), "The store no longer holds the destroyed entity");

            world.DestroyEntity(reused);
        }

        private void TestStaleHandleComponents()
        {
            Entity stale = world.CreateEntity();
            world.DestroyEntity(stale);
            Entity fresh = world.CreateEntity();
            world.AddComponent(fresh, new TestHealth { value = 42 });

            Check(!world.HasComponent<TestHealth>(stale), "A stale handle doesn't see the new entity's components");

            // Expected: AddComponent logs an error for the dead entity
            Check(!world.AddComponent(stale, new TestHealth { value = 1 }), "Add through a stale handle is refused");
            Check(world.TryGetComponent(fresh, out TestHealth health) && health.value == 42, "The new entity is untouched by the stale handle");

            world.DestroyEntity(fresh);
        }

        // ---------------------------------------------------------------- queries

        private const int QUERY_TEST_CAPACITY = 16;

        // Runs on fresh local worlds: counts don't depend on the entities of the game or of the other buttons
        [Button]
        public void TestQueries()
        {
            passed = 0;
            failed = 0;

            Run(nameof(TestMatchesAllAndNone), TestMatchesAllAndNone);
            Run(nameof(TestMatchesAny), TestMatchesAny);
            Run(nameof(TestQueryOnWorld), TestQueryOnWorld);
            Run(nameof(TestQueryGeneration), TestQueryGeneration);
            Run(nameof(TestQueryBatchDestroy), TestQueryBatchDestroy);
            Run(nameof(TestQueryOnlyActive), TestQueryOnlyActive);

            Debug.Log($"[ECSTest] Queries: {passed} passed, {failed} failed");
        }

        private static int CountMatches(World targetWorld, in EntityQuery query, bool onlyActive = false)
        {
            int count = 0;
            foreach (Entity _ in targetWorld.Query(query, onlyActive))
                count++;

            return count;
        }

        // Matches alone, without any World
        private void TestMatchesAllAndNone()
        {
            EntityQuery query = default(EntityQuery).WithAll<TestHealth>().Without<TestTag>();

            Check(query.Matches(ComponentMask.Of<TestHealth>().With<TestMove>()), "all + none: Health+Move is accepted");
            Check(!query.Matches(ComponentMask.Of<TestHealth>().With<TestTag>()), "all + none: Health+Tag is refused (none)");
            Check(!query.Matches(ComponentMask.Of<TestMove>()), "all + none: Move alone is refused (missing Health)");
            Check(!query.Matches(default), "all + none: an empty mask is refused");
            Check(default(EntityQuery).Matches(default), "An empty query accepts everything");
        }

        private void TestMatchesAny()
        {
            EntityQuery query = default(EntityQuery).WithAny<TestHealth>().WithAny<TestMove>();

            Check(query.Matches(ComponentMask.Of<TestHealth>()), "any: Health alone is accepted");
            Check(query.Matches(ComponentMask.Of<TestMove>()), "any: Move alone is accepted");
            Check(query.Matches(ComponentMask.Of<TestHealth>().With<TestMove>()), "any: Health+Move is accepted");
            Check(!query.Matches(ComponentMask.Of<TestTag>()), "any: Tag alone is refused");
            Check(!query.Matches(default), "any: an empty mask is refused");
        }

        private void TestQueryOnWorld()
        {
            var testWorld = new World(QUERY_TEST_CAPACITY);

            Entity healthAndMove = testWorld.CreateEntity();
            testWorld.AddComponent(healthAndMove, new TestHealth());
            testWorld.AddComponent(healthAndMove, new TestMove());

            Entity healthOnly = testWorld.CreateEntity();
            testWorld.AddComponent(healthOnly, new TestHealth());

            Entity healthAndTag = testWorld.CreateEntity();
            testWorld.AddComponent(healthAndTag, new TestHealth());
            testWorld.AddComponent(healthAndTag, new TestTag());

            EntityQuery query = default(EntityQuery).WithAll<TestHealth>().Without<TestTag>();

            int count = 0;
            bool foundHealthAndMove = false, foundHealthOnly = false, foundHealthAndTag = false;
            foreach (Entity entity in testWorld.Query(query))
            {
                count++;
                foundHealthAndMove |= entity == healthAndMove;
                foundHealthOnly |= entity == healthOnly;
                foundHealthAndTag |= entity == healthAndTag;
            }

            Check(count == 2, "Query returns exactly 2 entities");
            Check(foundHealthAndMove && foundHealthOnly, "Query returns Health+Move and Health only");
            Check(!foundHealthAndTag, "Query skips the entity excluded by Without<Tag>");
        }

        // Entities returned by a query carry their current generation
        private void TestQueryGeneration()
        {
            var testWorld = new World(QUERY_TEST_CAPACITY);

            Entity destroyed = testWorld.CreateEntity();
            testWorld.AddComponent(destroyed, new TestHealth());
            testWorld.DestroyEntity(destroyed);

            Entity reused = testWorld.CreateEntity();
            testWorld.AddComponent(reused, new TestHealth());

            int count = 0;
            bool returnedIsAlive = false, returnedIsReused = false;
            foreach (Entity entity in testWorld.Query(default(EntityQuery).WithAll<TestHealth>()))
            {
                count++;
                returnedIsAlive = testWorld.IsAlive(entity);
                returnedIsReused = entity == reused;
            }

            Check(reused.id == destroyed.id, "The id was reused");
            Check(count == 1, "Only the new entity is returned");
            Check(returnedIsAlive, "The returned entity is alive (generation included)");
            Check(returnedIsReused, "The returned entity equals the new handle, not the old one");
        }

        // Destroying during the foreach would break the iteration: collect first, destroy after
        private void TestQueryBatchDestroy()
        {
            var testWorld = new World(QUERY_TEST_CAPACITY);
            for (int i = 0; i < 5; i++)
                testWorld.AddComponent(testWorld.CreateEntity(), new TestHealth());

            EntityQuery query = default(EntityQuery).WithAll<TestHealth>();

            using (ListPool<Entity>.Get(out List<Entity> toDestroy))
            {
                foreach (Entity entity in testWorld.Query(query))
                    toDestroy.Add(entity);

                Check(toDestroy.Count == 5, "The 5 entities are collected");

                foreach (Entity entity in toDestroy)
                    testWorld.DestroyEntity(entity);
            }

            Check(CountMatches(testWorld, query) == 0, "No entity left after the batch destroy");
        }

        private void TestQueryOnlyActive()
        {
            var testWorld = new World(QUERY_TEST_CAPACITY);

            Entity active = testWorld.CreateEntity();
            testWorld.AddComponent(active, new TestHealth());

            Entity inactive = testWorld.CreateEntity();
            testWorld.AddComponent(inactive, new TestHealth());
            testWorld.Deactivate(inactive);

            EntityQuery query = default(EntityQuery).WithAll<TestHealth>();

            Check(CountMatches(testWorld, query) == 2, "Without onlyActive, both entities are returned");
            Check(CountMatches(testWorld, query, onlyActive: true) == 1, "With onlyActive, the inactive one is skipped");

            Entity first = Entity.None;
            foreach (Entity entity in testWorld.Query(query, onlyActive: true))
                first = entity;

            Check(first == active, "With onlyActive, the returned entity is the active one");
        }

        [Button]
        public void IsAlive(int index)
        {
            if (index < 0 || index >= current.Count)
            {
                Debug.LogWarning($"[ECSTest] Entity at {index} is out of range or negative]");
                return;
            }
            
            Debug.Log($"[ECSTest] Entity at {index} is alive ? : {world.IsAlive(current[index])}, with generations : {world.GetGeneration(current[index].id)})");
        }
    }
}