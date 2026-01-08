"use client";

import { useRouter, useSearchParams } from "next/navigation";

interface CityFilterProps {
  cities: string[];
  currentCity: string | null;
}

export default function CityFilter({ cities, currentCity }: CityFilterProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const handleCityChange = (city: string) => {
    const params = new URLSearchParams(searchParams.toString());
    
    if (city === "") {
      params.delete("city");
    } else {
      params.set("city", city);
    }
    
    // Reset to page 1 when changing city
    params.set("page", "1");
    
    router.push(`/?${params.toString()}`);
  };

  return (
    <div className="flex items-center gap-3">
      <label htmlFor="city-filter" className="text-sm font-medium text-gray-700">
        Şehir:
      </label>
      <select
        id="city-filter"
        value={currentCity || ""}
        onChange={(e) => handleCityChange(e.target.value)}
        className="px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 min-w-[200px]"
      >
        <option value="">Tüm Şehirler</option>
        {cities.map((city) => (
          <option key={city} value={city}>
            {city}
          </option>
        ))}
      </select>
      {currentCity && (
        <button
          onClick={() => handleCityChange("")}
          className="text-sm text-indigo-600 hover:text-indigo-800 hover:underline"
        >
          Filtreyi Temizle
        </button>
      )}
    </div>
  );
}
