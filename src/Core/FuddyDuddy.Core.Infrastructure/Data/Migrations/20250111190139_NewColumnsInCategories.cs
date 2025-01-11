using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuddyDuddy.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnsInCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "Categories",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "KeywordsLocal",
                table: "Categories",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            
            migrationBuilder.Sql(@"
                UPDATE Categories SET Keywords = 'elections, parliament, government, law, president, minister, opposition, debates, rally, conflict', KeywordsLocal = 'выборы, парламент, правительство, закон, президент, министр, оппозиция, дебаты, митинг, конфликт' WHERE Id = 1;
                UPDATE Categories SET Keywords = 'finance, business, market, investments, currency, exchange rate, stocks, oil, growth, decline, GDP, inflation', KeywordsLocal = 'финансы, бизнес, рынок, инвестиции, валюта, курс, акции, нефть, рост, спад, ВВП, инфляция' WHERE Id = 2;
                UPDATE Categories SET Keywords = 'social issues, demographics, migration, inequality, protests, traditions, culture, religion, education', KeywordsLocal = 'социальные проблемы, демография, миграция, неравенство, протесты, традиции, культура, религия, образование' WHERE Id = 3;
                UPDATE Categories SET Keywords = 'art, literature, music, cinema, theater, exhibitions, concerts, festivals, museums, monuments', KeywordsLocal = 'искусство, литература, музыка, кино, театр, выставки, концерты, фестивали, музеи, памятники' WHERE Id = 4;
                UPDATE Categories SET Keywords = 'competitions, championship, match, tournament, victory, defeat, record, team, athlete, olympics', KeywordsLocal = 'соревнования, чемпионат, матч, турнир, победа, поражение, рекорд, команда, атлет, олимпиада' WHERE Id = 5;
                UPDATE Categories SET Keywords = 'innovation, gadgets, internet, artificial intelligence, robots, software, science, research, development', KeywordsLocal = 'инновации, гаджеты, интернет, искусственный интеллект, роботы, программы, наука, исследования, разработки' WHERE Id = 6;
                UPDATE Categories SET Keywords = 'school, university, students, teachers, exams, programs, reforms, science', KeywordsLocal = 'школа, университет, студенты, учителя, экзамены, программы, реформы, наука' WHERE Id = 7;
                UPDATE Categories SET Keywords = 'medicine, health, hospital, doctor, drugs, diseases, pandemic, vaccination, prevention', KeywordsLocal = 'медицина, здоровье, больница, врач, лекарства, болезни, пандемия, вакцинация, профилактика' WHERE Id = 8;
                UPDATE Categories SET Keywords = 'environment, pollution, climate, ecology, emissions, recycling', KeywordsLocal = 'окружающая среда, загрязнение, климат, экология, выбросы, переработка' WHERE Id = 9;
                UPDATE Categories SET Keywords = 'travel, leisure, countries, cities, attractions, hotels, flights, visas, tours', KeywordsLocal = 'путешествия, отдых, страны, города, достопримечательности, отели, авиабилеты, визы, экскурсии' WHERE Id = 10;
                UPDATE Categories SET Keywords = 'farm, harvest, agriculture, food, livestock, crop farming, technology', KeywordsLocal = 'ферма, урожай, сельское хозяйство, продукты питания, животноводство, растениеводство, технологии' WHERE Id = 11;
                UPDATE Categories SET Keywords = 'entrepreneurship, corporate news, startups', KeywordsLocal = 'предпринимательство, корпоративные новости, стартапы' WHERE Id = 12;
                UPDATE Categories SET Keywords = 'international relations, conflicts, war, peace, diplomacy, cooperation, countries, organizations', KeywordsLocal = 'международные отношения, конфликты, война, мир, дипломатия, сотрудничество, страны, организации' WHERE Id = 13;
                UPDATE Categories SET Keywords = 'accident, disaster, fire, flood, earthquake, hurricane, terrorist attack, rescue, victims', KeywordsLocal = 'авария, катастрофа, пожар, наводнение, землетрясение, ураган, теракт, спасение, жертвы' WHERE Id = 14;
                UPDATE Categories SET Keywords = 'temperature, precipitation, wind, forecast, climate, storm, hurricane, sun, clouds', KeywordsLocal = 'температура, осадки, ветер, прогноз, климат, шторм, ураган, солнце, облачность' WHERE Id = 15;
                UPDATE Categories SET Keywords = 'other', KeywordsLocal = 'другое' WHERE Id = 16;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "KeywordsLocal",
                table: "Categories");
        }
    }
}
