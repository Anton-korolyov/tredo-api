using System;
using Tredo.Api.Data;

namespace Tredo.Api.Models
{

    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category { Id = 1, Name = "Electronics" },
                    new Category { Id = 2, Name = "Phones" },
                    new Category { Id = 3, Name = "Cars" },
                    new Category { Id = 4, Name = "Home" },
                    new Category { Id = 5, Name = "Furniture" },
                    new Category { Id = 6, Name = "Clothes" },
                    new Category { Id = 7, Name = "Other" }
                );
                if (!db.Cities.Any())
                {
                    db.Cities.AddRange(
                        new City { Id = 1, NameHe = "תל אביב", NameEn = "Tel Aviv", NameRu = "Тель-Авив" },
                        new City { Id = 2, NameHe = "חיפה", NameEn = "Haifa", NameRu = "Хайфа" },
                        new City { Id = 3, NameHe = "ירושלים", NameEn = "Jerusalem", NameRu = "Иерусалим" },
                        new City { Id = 4, NameHe = "ראשון לציון", NameEn = "Rishon LeZion", NameRu = "Ришон ле-Цион" },
                        new City { Id = 5, NameHe = "פתח תקווה", NameEn = "Petah Tikva", NameRu = "Петах-Тиква" },
                        new City { Id = 6, NameHe = "נתניה", NameEn = "Netanya", NameRu = "Нетания" },
                        new City { Id = 7, NameHe = "באר שבע", NameEn = "Be'er Sheva", NameRu = "Беэр-Шева" },
                        new City { Id = 8, NameHe = "אשדוד", NameEn = "Ashdod", NameRu = "Ашдод" },
                        new City { Id = 9, NameHe = "אשקלון", NameEn = "Ashkelon", NameRu = "Ашкелон" },
                        new City { Id = 10, NameHe = "קריית ים", NameEn = "Kiryat Yam", NameRu = "Кирьят-Ям" },
                        new City { Id = 11, NameHe = "קריית ביאליק", NameEn = "Kiryat Bialik", NameRu = "Кирьят-Бялик" },
                        new City { Id = 12, NameHe = "קריית מוצקין", NameEn = "Kiryat Motzkin", NameRu = "Кирьят-Моцкин" },
                        new City { Id = 13, NameHe = "קריית אתא", NameEn = "Kiryat Ata", NameRu = "Кирьят-Ата" }
                    );
                }
                db.SaveChanges();
            }
        }
    }

}
