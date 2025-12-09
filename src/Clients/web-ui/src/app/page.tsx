import { getServerSession } from "next-auth";
import Link from "next/link";
import EventCard from "@/components/EventCard";

type Event = {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  imageUrl: string;
  city: string;
};

async function getEvents() {
  try {
    const res = await fetch("http://localhost/api/events?page=1&pageSize=50", {
      cache: "force-cache",
      next: { revalidate: 300 }
    });

    if (!res.ok) {
      console.error("Failed to fetch events:", res.status, res.statusText);
      return [];
    }

    return res.json();
  } catch (error) {
    console.error("Error fetching events:", error);
    return [];
  }
}

async function getFollowedArtists(accessToken?: string) {
  if (!accessToken) return [];

  try {
    const res = await fetch("http://localhost/api/followers", {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
      cache: "no-store",
    });

    if (!res.ok) return [];

    const data = await res.json();
    return data.map((f: any) => f.artistId);
  } catch (error) {
    console.error("Error fetching follows:", error);
    return [];
  }
}

export default async function Home() {
  const session = await getServerSession();
  let events: Event[] = [];
  let followedArtists: string[] = [];

  try {
    events = await getEvents();
    if (session && (session as any).accessToken) {
      followedArtists = await getFollowedArtists((session as any).accessToken);
    }
  } catch (error) {
    console.error("Error in Home:", error);
  }

  if (!Array.isArray(events)) {
    events = [];
  }

  return (
    <main className="min-h-screen bg-gray-100 p-8">
      <header className="flex justify-between items-center mb-8 bg-white p-4 rounded-lg shadow">
        <h1 className="text-2xl font-bold text-indigo-600">Etkinlik Radar</h1>
        <div>
          {session ? (
            <div className="flex items-center gap-4">
              <span className="text-sm">Hoşgeldin, {session.user?.name}</span>
              <Link href="/api/auth/signout" className="text-red-500 hover:underline">Çıkış Yap</Link>
            </div>
          ) : (
            <Link href="/api/auth/signin" className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700">
              Giriş Yap
            </Link>
          )}
        </div>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {events.map((evt, index) => (
          <EventCard
            key={`${evt.id}-${index}`}
            event={evt}
            isFollowing={followedArtists.includes(evt.name)}
          />
        ))}
      </div>
    </main>
  );
}
