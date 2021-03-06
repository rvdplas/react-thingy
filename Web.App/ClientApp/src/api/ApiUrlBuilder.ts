export class ApiUrlBuilder {
    private baseUrl: string;

    public constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    public getStarWarsPeople(): string {
        return `${this.baseUrl}/api/starwars/people`;
    }
}