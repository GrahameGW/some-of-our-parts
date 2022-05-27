using UnityEngine;
using System;
using System.Linq;


namespace FictionalOctoDoodle.Core
{
    public class LimbAssembly : MonoBehaviour
    {
        public Action OnLimbConfigChanged;

        [SerializeField] LimbConnector torso;
        [SerializeField] LimbConnector neckArm;
        [SerializeField] LimbConnector neckLeg;
        [SerializeField] LimbConnector frontArm;
        [SerializeField] LimbConnector backArm;
        [SerializeField] LimbConnector frontLeg;
        [SerializeField] LimbConnector backLeg;

        [SerializeField] LimbAnimationControllers controllers;
        [SerializeField] Collider2D legCollider;

        private Animator animator;


        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = SetAnimationController();
            legCollider.enabled = false;
        }

        public void FlipModels(float degrees)
        {
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    degrees,
                    transform.eulerAngles.z
                    );
        }
        public bool TryAddLimb(LimbData limb)
        {
           
            if (limb.Slots.Contains(LimbSlot.Torso))
            {
                if (torso.limbData) return false;

                torso.ClearLoadedLimb();
                AssembleLimb(limb, torso);
                return true;
            }

            if (torso.limbData == null)
            {
                // do neck slot
                return true;
            }

            if (frontLeg.limbData == null && limb.Slots.Contains(LimbSlot.FrontLeg))
            {
                AssembleLimb(limb, frontLeg);
                return true;
            }
                
            if (backLeg.limbData == null && limb.Slots.Contains(LimbSlot.BackLeg))
            {
                AssembleLimb(limb, backLeg);
                return true;
            }

            if (frontArm.limbData == null && limb.Slots.Contains(LimbSlot.FrontArm))
            {
                AssembleLimb(limb, frontArm);
                return true;
            }

            if (backArm.limbData == null && limb.Slots.Contains(LimbSlot.BackArm))
            {
                AssembleLimb(limb, backArm);
                return true;
            }

            return false;
        }

        private void AssembleLimb(LimbData limb, LimbConnector slot)
        {
            slot.limbData = limb;

            var obj = Instantiate(limb.Prefab, transform);
            obj.name = SetLimbName(slot.slotId);
            if (slot.slotId == LimbSlot.BackArm || slot.slotId == LimbSlot.BackLeg)
            {
                LabelBackLimbs(obj.transform);
            }
            obj.transform.localPosition = slot.position;
            obj.GetComponent<SpriteRenderer>().sortingOrder = slot.orderInLayer;
            slot.limbObj = obj;

            var newController = SetAnimationController();
            animator.runtimeAnimatorController = null;
            animator.runtimeAnimatorController = newController;
            OnLimbConfigChanged?.Invoke();

            legCollider.enabled = frontLeg.limbData != null;
        }

        private RuntimeAnimatorController SetAnimationController()
        {
            if (torso.limbData == null)
            {
                if (neckArm.limbData == null && neckLeg.limbData == null)
                {
                    return controllers.skullOnly;
                }

                return neckArm.limbData != null ?
                    controllers.skullNeckArm :
                    controllers.skullNeckLeg;
            }

            if (backArm.limbData != null)
            {
                if (backLeg.limbData != null)
                {
                    return controllers.fullBody;
                }

                return frontLeg.limbData != null ?
                    controllers.oneLegTwoArm :
                    controllers.torsoTwoArm;
            }

            if (frontArm.limbData != null)
            {
                if (backLeg.limbData != null)
                {
                    return controllers.twoLegOneArm;
                }

                return frontLeg.limbData != null ?
                    controllers.oneLegOneArm :
                    controllers.torsoOneArm;
            }

            if (backLeg.limbData != null)
            {
                return controllers.torsoTwoLeg;
            }

            return frontLeg.limbData != null ?
                controllers.torsoOneLeg :
                controllers.skullTorso;
        }

        private void LabelBackLimbs(Transform limb)
        {
            if (limb.childCount == 0) return;
            foreach (Transform child in limb)
            {
                child.name += "Back"; // so the animator works later 
                LabelBackLimbs(child); // recursion muthafucka
            }
        }

        private string SetLimbName(LimbSlot slot)
        {
            return slot switch
            {
                LimbSlot.Torso => "SkeletonTorso",
                LimbSlot.FrontLeg => "SkeletonLeg",
                LimbSlot.BackLeg => "SkeletonLegBack",
                LimbSlot.BackArm => "SkeletonArmBack",
                LimbSlot.FrontArm => "SkeletonArm",
                _ => "No Slot Found",
            };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere((Vector2)transform.position + torso.position, 0.25f);
            Gizmos.DrawSphere((Vector2)transform.position + frontArm.position, 0.25f);
            Gizmos.DrawSphere((Vector2)transform.position + backArm.position, 0.25f);
            Gizmos.DrawSphere((Vector2)transform.position + frontLeg.position, 0.25f);
            Gizmos.DrawSphere((Vector2)transform.position + backLeg.position, 0.25f);
        }

        [Serializable]
        public class LimbConnector
        {
            public LimbSlot slotId;
            public Vector2 position;
            public int orderInLayer;
            public LimbData limbData;
            public GameObject limbObj;

            public void ClearLoadedLimb()
            {
                limbData = null;
                Destroy(limbObj);
                limbObj = null;
            }
        }

        [Serializable]
        public class LimbAnimationControllers
        {
            public RuntimeAnimatorController skullOnly;
            public RuntimeAnimatorController skullTorso;
            public RuntimeAnimatorController skullNeckArm;
            public RuntimeAnimatorController skullNeckLeg;
            public RuntimeAnimatorController torsoOneArm;
            public RuntimeAnimatorController torsoTwoArm;
            public RuntimeAnimatorController torsoOneLeg;
            public RuntimeAnimatorController torsoTwoLeg;
            public RuntimeAnimatorController oneLegOneArm;
            public RuntimeAnimatorController oneLegTwoArm;
            public RuntimeAnimatorController twoLegOneArm;
            public RuntimeAnimatorController fullBody;
        }
    }


}
