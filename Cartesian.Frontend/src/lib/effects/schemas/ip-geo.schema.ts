import { Schema } from "effect";

export const IpGeoSchema = Schema.Struct({
	status: Schema.String,
	countryCode: Schema.String,
	lat: Schema.Number,
	lon: Schema.Number,
});

export type IpGeo = typeof IpGeoSchema.Type;

export class IpGeoError extends Schema.TaggedError<IpGeoError>()("IpGeoError", {
	message: Schema.String,
}) {}
