"use client";

import FollowButton from "./FollowButton";

type Event = {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  imageUrl: string;
  city: string;
  ticketUrl?: string;
  source?: string;
};

interface EventCardProps {
  event: Event;
  isFollowing: boolean;
}

export default function EventCard({ event, isFollowing }: EventCardProps) {
  return (
    <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-lg transition flex flex-col">
      <img
        src={event.imageUrl}
        alt={event.name}
        className="w-full h-48 object-cover"
      />
      <div className="p-4 flex-1 flex flex-col">
        <h2 className="font-bold text-xl mb-2 text-gray-800">{event.name}</h2>
        <div className="text-gray-600 text-sm mb-2">
          {event.city} - {new Date(event.eventDate).toLocaleDateString()}
        </div>
        <div className="text-gray-500 text-sm line-clamp-2 mb-4 flex-1">
          {event.description}
        </div>

        <div className="mt-auto flex flex-col gap-2">
          {event.ticketUrl && (
            <a
              href={event.ticketUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="w-full bg-green-600 text-white py-2 rounded hover:bg-green-700 font-medium text-center"
            >
              Bilet Al ({event.source})
            </a>
          )}
          <FollowButton
            artistId={event.name}
            initialIsFollowing={isFollowing}
          />
        </div>
      </div>
    </div>
  );
}
