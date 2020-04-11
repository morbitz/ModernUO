using System;
using System.Collections.Generic;
using Server.Engines.Plants;
using Server.Items;

namespace Server.Mobiles
{
  public class BakeKitsune : BaseCreature
  {
    private static Dictionary<Mobile, ExpireTimer> m_Table = new Dictionary<Mobile, ExpireTimer>();

    [Constructible]
    public BakeKitsune() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
    {
      Body = 246;

      SetStr(171, 220);
      SetDex(126, 145);
      SetInt(376, 425);

      SetHits(301, 350);

      SetDamage(15, 22);

      SetDamageType(ResistanceType.Physical, 70);
      SetDamageType(ResistanceType.Energy, 30);

      SetResistance(ResistanceType.Physical, 40, 60);
      SetResistance(ResistanceType.Fire, 70, 90);
      SetResistance(ResistanceType.Cold, 40, 60);
      SetResistance(ResistanceType.Poison, 40, 60);
      SetResistance(ResistanceType.Energy, 40, 60);

      SetSkill(SkillName.EvalInt, 80.1, 90.0);
      SetSkill(SkillName.Magery, 80.1, 90.0);
      SetSkill(SkillName.MagicResist, 80.1, 100.0);
      SetSkill(SkillName.Tactics, 70.1, 90.0);
      SetSkill(SkillName.Wrestling, 50.1, 55.0);

      Fame = 8000;
      Karma = -8000;

      Tamable = true;
      ControlSlots = 2;
      MinTameSkill = 80.7;


      if (Utility.RandomDouble() < .25)
        PackItem(Seed.RandomBonsaiSeed());
    }

    public BakeKitsune(Serial serial) : base(serial)
    {
    }

    public override string CorpseName => "a bake kitsune corpse";
    public override string DefaultName => "a bake kitsune";

    public override int Meat => 5;
    public override int Hides => 10;
    public override HideType HideType => HideType.Barbed;
    public override FoodType FavoriteFood => FoodType.Fish;
    public override bool ShowFameTitle => false;
    public override bool ClickTitle => false;
    public override bool PropertyTitle => false;

    public override void GenerateLoot()
    {
      AddLoot(LootPack.FilthyRich);
      AddLoot(LootPack.Rich);
      AddLoot(LootPack.MedScrolls, 2);
    }

    public override void OnCombatantChange()
    {
      if (Combatant == null && !IsBodyMod && !Controlled && m_DisguiseTimer == null && Utility.RandomBool())
        m_DisguiseTimer = Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(15, 30)), Disguise);
    }

    public override bool OnBeforeDeath()
    {
      RemoveDisguise();

      return base.OnBeforeDeath();
    }

    public override void OnGaveMeleeAttack(Mobile defender)
    {
      base.OnGaveMeleeAttack(defender);

      if (0.1 <= Utility.RandomDouble())
        return;

      /* Blood Bath
         * Start cliloc 1070826
         * Sound: 0x52B
         * 2-3 blood spots
         * Damage: 2 hps per second for 5 seconds
         * End cliloc: 1070824
         */

      if (m_Table.TryGetValue(defender, out ExpireTimer timer))
      {
        timer.DoExpire();
        defender.SendLocalizedMessage(1070825); // The creature continues to rage!
      }
      else
      {
        defender.SendLocalizedMessage(1070826); // The creature goes into a rage, inflicting heavy damage!
      }

      timer = new ExpireTimer(defender, this);
      timer.Start();
      m_Table[defender] = timer;
    }

    public override int GetAngerSound() => 0x4DE;

    public override int GetIdleSound() => 0x4DD;

    public override int GetAttackSound() => 0x4DC;

    public override int GetHurtSound() => 0x4DF;

    public override int GetDeathSound() => 0x4DB;

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);
      writer.Write(1);
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);
      int version = reader.ReadInt();

      if (version == 0 && PhysicalResistance > 60)
      {
        SetResistance(ResistanceType.Physical, 40, 60);
        SetResistance(ResistanceType.Fire, 70, 90);
        SetResistance(ResistanceType.Cold, 40, 60);
        SetResistance(ResistanceType.Poison, 40, 60);
        SetResistance(ResistanceType.Energy, 40, 60);
      }

      Timer.DelayCall(TimeSpan.Zero, RemoveDisguise);
    }

    private class ExpireTimer : Timer
    {
      private int m_Count;
      private Mobile m_From;
      private Mobile m_Mobile;

      public ExpireTimer(Mobile m, Mobile from) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
      {
        m_Mobile = m;
        m_From = from;
        Priority = TimerPriority.TwoFiftyMS;
      }

      public void DoExpire()
      {
        Stop();
        m_Table.Remove(m_Mobile);
      }

      public void DrainLife()
      {
        if (m_Mobile.Alive)
          m_Mobile.Damage(2, m_From);
        else
          DoExpire();
      }

      protected override void OnTick()
      {
        DrainLife();

        if (++m_Count >= 5)
        {
          DoExpire();
          m_Mobile.SendLocalizedMessage(1070824); // The creature's rage subsides.
        }
      }
    }

    #region Disguise

    private Timer m_DisguiseTimer;

    public void Disguise()
    {
      if (Combatant != null || IsBodyMod || Controlled)
        return;

      FixedEffect(0x376A, 8, 32);
      PlaySound(0x1FE);

      Female = Utility.RandomBool();

      if (Female)
      {
        BodyMod = 0x191;
        Name = NameList.RandomName("female");
      }
      else
      {
        BodyMod = 0x190;
        Name = NameList.RandomName("male");
      }

      Title = "the mystic llama herder";
      Hue = Race.Human.RandomSkinHue();
      HairItemID = Race.Human.RandomHair(this);
      HairHue = Race.Human.RandomHairHue();
      FacialHairItemID = Race.Human.RandomFacialHair(this);
      FacialHairHue = HairHue;

      switch (Utility.Random(4))
      {
        case 0:
          AddItem(new Shoes(Utility.RandomNeutralHue()));
          break;
        case 1:
          AddItem(new Boots(Utility.RandomNeutralHue()));
          break;
        case 2:
          AddItem(new Sandals(Utility.RandomNeutralHue()));
          break;
        case 3:
          AddItem(new ThighBoots(Utility.RandomNeutralHue()));
          break;
      }

      AddItem(new Robe(Utility.RandomNondyedHue()));

      m_DisguiseTimer = Timer.DelayCall(TimeSpan.FromSeconds(75), RemoveDisguise);
    }

    public void RemoveDisguise()
    {
      if (!IsBodyMod)
        return;

      Name = null;
      Title = null;
      BodyMod = 0;
      Hue = 0;
      HairItemID = 0;
      HairHue = 0;
      FacialHairItemID = 0;
      FacialHairHue = 0;

      DeleteItemOnLayer(Layer.OuterTorso);
      DeleteItemOnLayer(Layer.Shoes);

      m_DisguiseTimer = null;
    }

    public void DeleteItemOnLayer(Layer layer)
    {
      FindItemOnLayer(layer)?.Delete();
    }

    #endregion
  }
}