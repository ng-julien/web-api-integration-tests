﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

#nullable disable

namespace Zoo.Infrastructure.Entities.Zoo
{
    public partial class Food
    {
        public Food()
        {
            AnimalCanEats = new HashSet<AnimalCanEat>();
            AnimalEats = new HashSet<AnimalEat>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AnimalCanEat> AnimalCanEats { get; set; }
        public virtual ICollection<AnimalEat> AnimalEats { get; set; }
    }
}