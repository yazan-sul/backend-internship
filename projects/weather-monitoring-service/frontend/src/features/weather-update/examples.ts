export const weatherExamples = {
  JSON: `{
  "Location": "Jericho",
  "Temperature": 35,
  "Humidity": 80
}`,
  XML: `<WeatherData>
  <Location>Bethlehem</Location>
  <Temperature>18.5</Temperature>
  <Humidity>62</Humidity>
</WeatherData>`,
} as const;

export type WeatherExampleFormat = keyof typeof weatherExamples;
